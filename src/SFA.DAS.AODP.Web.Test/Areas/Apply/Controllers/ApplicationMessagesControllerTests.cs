using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Aodp.Domain.Files;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Application.Commands.Files;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Infrastructure.Common.IO;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Common;
using SFA.DAS.AODP.Models.Exceptions;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Apply.Controllers;
using SFA.DAS.AODP.Web.Areas.Apply.Models;
using SFA.DAS.AODP.Web.Helpers.File;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.RelatedLinks;
using System.Text;

namespace SFA.DAS.AODP.Web.Test.Areas.Apply.Controllers
{
    public class ApplicationMessagesControllerTests
    {
        private readonly Fixture _fixture = new();

        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ILogger<ApplicationMessagesController>> _loggerMock = new();
        private readonly Mock<IUserHelperService> _userHelperServiceMock = new();
        private readonly Mock<IMessageFileValidationService> _messageFileValidationService = new();
        private readonly Mock<IFileService> _fileService = new();

        private readonly FormBuilderSettings _formBuilderSettings;
        private readonly FileUploadValidator _fileUploadValidator;

        private readonly ApplicationMessagesController _controller;


        public ApplicationMessagesControllerTests()
        {
            _formBuilderSettings = new FormBuilderSettings
            {
                MaxUploadFileSize = 10,
                UploadFileTypesAllowed = new List<string> { ".docx", ".pdf", ".xlsx" }
            };

            _fileUploadValidator = new FileUploadValidator(_formBuilderSettings);

            _controller = new ApplicationMessagesController(
                _mediatorMock.Object,
                _loggerMock.Object,
                _userHelperServiceMock.Object,
                _messageFileValidationService.Object,
                _formBuilderSettings,
                _fileService.Object,
                _fileUploadValidator);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var urlMock = new Mock<IUrlHelper>();


            urlMock
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns("/fake-url");

            urlMock
                .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns("/fake-url");

            _controller.Url = urlMock.Object;

            _controller.TempData = new TempDataDictionary(
                _controller.HttpContext,
                Mock.Of<ITempDataProvider>());
        }



        [Fact]
        public async Task ApplicationMessages_ValidFiles_Uploaded()
        {
            // Arrange
            var fileName = "test.docx";
            var contentType = "application/msword";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

            var formFile = new Mock<IFormFile>();
            formFile.SetupGet(f => f.FileName).Returns(fileName);
            formFile.SetupGet(f => f.ContentType).Returns(contentType);
            formFile.Setup(f => f.OpenReadStream()).Returns(stream);

            var model = new ApplicationMessagesViewModel
            {
                AdditionalActions = new() { Send = true },
                Files = new() { formFile.Object }
            };

            var applicationId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();
            var messageId = Guid.NewGuid();

            _messageFileValidationService
                .Setup(v => v.ValidateFiles(It.IsAny<List<IFormFile>>()));

            _fileService
                .Setup(f => f.UploadAsync(
                    FileCategory.MessageAttachment,
                    It.Is<FileContext>(c =>
                        c.ApplicationId == applicationId &&
                        c.MessageId == messageId),
                    fileName,
                    contentType,
                    It.IsAny<Stream>()))
                .ReturnsAsync(new FileStorageLocation("files", "messages/blob-path"));

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<CreateApplicationMessageCommand>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new BaseMediatrResponse<CreateApplicationMessageCommandResponse>
                    {
                        Success = true,
                        Value = new CreateApplicationMessageCommandResponse
                        {
                            Id = messageId,
                            EmailSent = false
                        }
                    }));

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateFileMetadataCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<EmptyResponse>
                {
                    Success = true,
                    Value = new EmptyResponse()
                });

            _controller.TempData = new Mock<ITempDataDictionary>().Object;

            // Act
            var result = await _controller.ApplicationMessages(
                model, applicationId, organisationId, formVersionId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);

            _fileService.Verify(f =>
                f.UploadAsync(
                    FileCategory.MessageAttachment,
                    It.IsAny<FileContext>(),
                    fileName,
                    contentType,
                    It.IsAny<Stream>()),
                Times.Once);
        }

        [Fact]
        public async Task ApplicationMessages_InvalidFiles_ErrorViewModelReturned()
        {
            // Arrange
            var formFile = new Mock<IFormFile>();

            ApplicationMessagesViewModel model = new()
            {
                AdditionalActions = new()
                {
                    Send = true
                },
                Files = new()
                {
                    formFile.Object
                }
            };
            var applicationId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();
            _controller.TempData = new Mock<ITempDataDictionary>().Object;

            _messageFileValidationService
                .Setup(f => f.ValidateFiles(model.Files))
                .Throws(new FileUploadPolicyException(FileUploadRejectionReason.InvalidFileName));

            // Act
            var result = await _controller.ApplicationMessages(model, applicationId, organisationId, formVersionId);


            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultViewModel = Assert.IsAssignableFrom<ApplicationMessagesViewModel>(viewResult.ViewData.Model);
            Assert.True(resultViewModel.AdditionalActions.Preview);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Files", _controller.ModelState.Keys);
        }


        [Fact]
        public async Task ApplicationMessageFileDownload_Success_ReturnsFile()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            var fileMetadata = new FileMetadataDto
            {
                FileId = fileId,
                FileName = "test.docx",
                BlobContainer = "files",
                BlobPath = "messages/test.docx",
                ContentType = "application/msword",
                ApplicationId = applicationId,
                MessageId = messageId,
                IsDownloadable = true
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetFileMetadataQuery>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new BaseMediatrResponse<GetFileMetadataQueryResponse>
                    {
                        Success = true,
                        Value = new GetFileMetadataQueryResponse
                        {
                            Files = new List<FileMetadataDto> { fileMetadata }
                        }
                    }));

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetApplicationMessageByIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new BaseMediatrResponse<GetApplicationMessageByIdQueryResponse>
                    {
                        Success = true,
                        Value = new GetApplicationMessageByIdQueryResponse
                        {
                            SharedWithAwardingOrganisation = true
                        }
                    }));

            var content = "Test file content";
            _fileService
                .Setup(f => f.OpenReadStreamAsync("files", "messages/test.docx"))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)));

            // Act
            var result = await _controller.ApplicationMessageFileDownload(
                applicationId, messageId, fileId);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/msword", fileResult.ContentType);
            Assert.Equal("test.docx", fileResult.FileDownloadName);
        }


        [Fact]
        public async Task ApplicationMessageFileDownload_MessageNotSharedWithAo_ReturnsBadRequest()
        {
            // Arrange
            var messageId = _fixture.Create<Guid>();
            var applicationId = _fixture.Create<Guid>();
            var fileId = Guid.NewGuid();

            BaseMediatrResponse<GetApplicationMessageByIdQueryResponse> response = new()
            {
                Value = new() { SharedWithAwardingOrganisation = false },
                Success = true,
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationMessageByIdQuery>(), default)).ReturnsAsync(response);

            // Act
            var result = await _controller.ApplicationMessageFileDownload(applicationId, messageId, fileId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ApplicationMessageFileDownload_InvalidFilePath_ReturnsBadRequest()
        {
            // Arrange
            var applicationId = _fixture.Create<Guid>();
            var fileId = Guid.Empty;
            var messageId = _fixture.Create<Guid>();

            // Act
            var result = await _controller.ApplicationMessageFileDownload(applicationId, messageId, fileId);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ApplicationMessages_EmailSentTrue_SetsMessageSentBanner()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();

            var msgResponse = new BaseMediatrResponse<CreateApplicationMessageCommandResponse>
            {
                Success = true,
                Value = new CreateApplicationMessageCommandResponse
                {
                    Id = Guid.NewGuid(),
                    EmailSent = true
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateApplicationMessageCommand>(), default))
                .ReturnsAsync(msgResponse);

            var model = new ApplicationMessagesViewModel
            {
                AdditionalActions = new() { Send = true }
            };

            // use a real TempDataDictionary to inspect results
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.ApplicationMessages(model, applicationId, organisationId, formVersionId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(_controller.TempData.ContainsKey(ApplicationMessagesController.NotificationKeys.MessageSentBanner.ToString()));
        }

        [Fact]
        public async Task ApplicationMessages_EmailSentFalse_DoesNotSetMessageSentBanner()
        {
            // Arrange
            var applicationId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();

            var msgResponse = new BaseMediatrResponse<CreateApplicationMessageCommandResponse>
            {
                Success = true,
                Value = new CreateApplicationMessageCommandResponse
                {
                    Id = Guid.NewGuid(),
                    EmailSent = false
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateApplicationMessageCommand>(), default))
                .ReturnsAsync(msgResponse);

            var model = new ApplicationMessagesViewModel
            {
                AdditionalActions = new() { Send = true }
            };

            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.ApplicationMessages(model, applicationId, organisationId, formVersionId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.False(_controller.TempData.ContainsKey(ApplicationMessagesController.NotificationKeys.MessageSentBanner.ToString()));
        }

        [Fact]
        public async Task Get_ApplicationMessages_SetsRelatedLinks()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var formVersionId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetApplicationMessagesByApplicationIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new BaseMediatrResponse<GetApplicationMessagesByApplicationIdQueryResponse>
                    {
                        Success = true,
                        Value = new GetApplicationMessagesByApplicationIdQueryResponse
                        {
                            Messages = new List<GetApplicationMessagesByApplicationIdQueryResponse.ApplicationMessage>()
                        }
                    }));

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetFileMetadataQuery>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new BaseMediatrResponse<GetFileMetadataQueryResponse>
                    {
                        Success = true,
                        Value = new GetFileMetadataQueryResponse
                        {
                            Files = new List<FileMetadataDto>()
                        }
                }));

            _controller.TempData =
                new TempDataDictionary(
                    new DefaultHttpContext(),
                    Mock.Of<ITempDataProvider>());

            // Act
            var result = await _controller.ApplicationMessages(
                organisationId,
                applicationId,
                formVersionId);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ApplicationMessagesViewModel>(view.Model);

            Assert.NotNull(model.RelatedLinks);
            Assert.NotEmpty(model.RelatedLinks);

            // Contextual link exists for ApplyApplicationMessages
            Assert.Contains(
                model.RelatedLinks,
                l => l.Text == RelatedLinkConstants.Text.ViewApplication);

            // AwardingOrganisation user type adds configured links
            Assert.Contains(
                model.RelatedLinks,
                l => l.Text == RelatedLinksConfiguration.FundingApprovalManual.Text);
        }
    }

}
