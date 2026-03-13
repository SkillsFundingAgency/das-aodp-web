using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Application.Queries.Application.Application;
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
        private readonly FormBuilderSettings _formBuilderSettings = new();

        private readonly Mock<IUserHelperService> _userHelperServiceMock = new();
        private readonly ApplicationMessagesController _controller;
        private readonly Mock<ILogger<ApplicationMessagesController>> _loggerMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();

        private readonly Mock<IMessageFileValidationService> _messageFileValidationService = new();
        private readonly Mock<IFileService> _fileService = new();


        public ApplicationMessagesControllerTests()
        {
            _controller = new(_mediatorMock.Object, _loggerMock.Object, _userHelperServiceMock.Object, _messageFileValidationService.Object, _formBuilderSettings, _fileService.Object);

            SetupControllerUrl();
        }

        [Fact]
        public async Task ApplicationMessages_ValidFiles_Uploaded()
        {
            // Arrange
            var fileName = _fixture.Create<string>();
            var contentType = _fixture.Create<string>();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            var formFile = new Mock<IFormFile>();

            formFile.SetupGet(f => f.FileName).Returns(fileName);
            formFile.SetupGet(f => f.ContentType).Returns(contentType);
            formFile.Setup(f => f.OpenReadStream()).Returns(stream);


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

            var msgResponse = new BaseMediatrResponse<CreateApplicationMessageCommandResponse>()
            {
                Success = true,
                Value = _fixture.Create<CreateApplicationMessageCommandResponse>()
            };

            var metaDataResponse = new BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>()
            {
                Success = true,
                Value = _fixture.Create<GetApplicationMetadataByIdQueryResponse>()
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateApplicationMessageCommand>(), default)).ReturnsAsync(msgResponse);
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationMetadataByIdQuery>(), default)).ReturnsAsync(metaDataResponse);

            _controller.TempData = new Mock<ITempDataDictionary>().Object;

            // Act
            var result = await _controller.ApplicationMessages(model, applicationId, organisationId, formVersionId);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);

            _fileService.Verify(f => f.UploadFileAsync($"messages/{applicationId}/{msgResponse.Value.Id}", It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));
            _fileService.Verify(f => f.UploadFileAsync(It.IsAny<string>(), fileName, It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()));
            _fileService.Verify(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), stream, It.IsAny<string>(), It.IsAny<string>()));
            _fileService.Verify(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), contentType, It.IsAny<string>()));
            _fileService.Verify(f => f.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), metaDataResponse.Value.Reference.ToString().PadLeft(6, '0')));

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
            var applicationId = _fixture.Create<Guid>();
            var messageId = _fixture.Create<Guid>();
            var file = $"messages/{applicationId}/{messageId}/";

            var blob = _fixture.Create<UploadedBlob>();
            string content = "Test file content";

            BaseMediatrResponse<GetApplicationMessageByIdQueryResponse> response = new()
            {
                Value = new() { SharedWithAwardingOrganisation = true },
                Success = true,
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationMessageByIdQuery>(), default)).ReturnsAsync(response);

            _fileService.Setup(f => f.GetBlobDetails(file)).ReturnsAsync(blob);
            _fileService.Setup(fs => fs.OpenReadStreamAsync(It.IsAny<string>()))
              .ReturnsAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)));

            // Act
            var result = await _controller.ApplicationMessageFileDownload(file, applicationId, messageId);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Equal(blob.FileName, fileResult.FileDownloadName);

            using StreamReader reader = new(fileResult.FileStream);
            Assert.Equal(content, reader.ReadToEnd());
        }

        [Fact]
        public async Task ApplicationMessageFileDownload_MessageNotSharedWithAo_ReturnsBadRequest()
        {
            // Arrange
            var messageId = _fixture.Create<Guid>();
            var applicationId = _fixture.Create<Guid>();
            var file = $"messages/{applicationId}/{messageId}/";

            BaseMediatrResponse<GetApplicationMessageByIdQueryResponse> response = new()
            {
                Value = new() { SharedWithAwardingOrganisation = false },
                Success = true,
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationMessageByIdQuery>(), default)).ReturnsAsync(response);

            // Act
            var result = await _controller.ApplicationMessageFileDownload(file, applicationId, messageId);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ApplicationMessageFileDownload_InvalidFilePath_ReturnsBadRequest()
        {
            // Arrange
            var applicationId = _fixture.Create<Guid>();
            var file = $"messages/{Guid.NewGuid()}/";
            var messageId = _fixture.Create<Guid>();

            // Act
            var result = await _controller.ApplicationMessageFileDownload(file, applicationId, messageId);

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


            var response = new BaseMediatrResponse<GetApplicationMessagesByApplicationIdQueryResponse>
            {
                Success = true,
                Value = new GetApplicationMessagesByApplicationIdQueryResponse()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationMessagesByApplicationIdQuery>(), default))
                .ReturnsAsync(response);

            // No blobs
            _fileService
                .Setup(s => s.ListBlobs(It.IsAny<string>()))
                .Returns(new List<UploadedBlob>());

            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await _controller.ApplicationMessages(organisationId, applicationId, formVersionId);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ApplicationMessagesViewModel>(view.Model);

            Assert.NotNull(model.RelatedLinks);
            Assert.NotEmpty(model.RelatedLinks);

            // Contextual link exists for ApplyApplicationMessages
            Assert.Contains(model.RelatedLinks, l => l.Text == RelatedLinkConstants.Text.ViewApplication);

            // AwardingOrganisation user type adds 3 configured links
            Assert.Contains(model.RelatedLinks, l => l.Text == RelatedLinksConfiguration.FundingApprovalManual.Text);
        }


        private void SetupControllerUrl()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var url = new Mock<IUrlHelper>();

            url.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
               .Returns("/fake-url");

            url.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
               .Returns("/fake-url");

            _controller.Url = url.Object;
        }


    }

}
