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
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationMessage;
using SFA.DAS.AODP.Web.Helpers.File;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.RelatedLinks;
using System.Text;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers
{
    public class ApplicationMessagesControllerTests
    {
        private readonly Fixture _fixture = new();

        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ILogger<ApplicationMessagesController>> _loggerMock = new();
        private readonly Mock<IUserHelperService> _userHelperServiceMock = new();
        private readonly Mock<IMessageFileValidationService> _messageFileValidationServiceMock = new();
        private readonly Mock<IFileService> _fileServiceMock = new();

        private readonly ApplicationMessagesController _controller;

        public ApplicationMessagesControllerTests()
        {
            var formBuilderSettings = new FormBuilderSettings
            {
                MaxUploadFileSize = 10,
                UploadFileTypesAllowed = new() { ".pdf", ".docx", ".xlsx" }
            };

            var fileUploadValidator = new FileUploadValidator(formBuilderSettings);

            _userHelperServiceMock
                .Setup(u => u.GetUserType())
                .Returns(UserType.Ofqual);

            _userHelperServiceMock
                .Setup(u => u.GetUserEmail())
                .Returns("review@test.com");

            _userHelperServiceMock
                .Setup(u => u.GetUserDisplayName())
                .Returns("Review User");

            _controller = new ApplicationMessagesController(
                _mediatorMock.Object,
                _loggerMock.Object,
                _userHelperServiceMock.Object,
                formBuilderSettings,
                _messageFileValidationServiceMock.Object,
                _fileServiceMock.Object,
                fileUploadValidator);

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

        // -----------------------------
        // ApplicationMessagesAsync (GET)
        // -----------------------------

        [Fact]
        public async Task ApplicationMessagesAsync_SetsRelatedLinks()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                    {
                        ApplicationId = applicationId,
                        SharedWithOfqual = true
                    }
                });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationMessagesByApplicationIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationMessagesByApplicationIdQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationMessagesByApplicationIdQueryResponse()
                });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetFileMetadataQueryResponse>
                {
                    Success = true,
                    Value = new GetFileMetadataQueryResponse()
                });

            var result = await _controller.ApplicationMessagesAsync(applicationReviewId);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ApplicationMessagesViewModel>(view.Model);

            Assert.NotNull(model.RelatedLinks);
            Assert.NotEmpty(model.RelatedLinks);
        }

        // -----------------------------
        // ApplicationMessages (POST)
        // -----------------------------

        [Fact]
        public async Task ApplicationMessages_EmailSentTrue_SetsBanner()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                    {
                        ApplicationId = applicationId,
                        SharedWithOfqual = true
                    }
                });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateApplicationMessageCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<CreateApplicationMessageCommandResponse>
                {
                    Success = true,
                    Value = new CreateApplicationMessageCommandResponse
                    {
                        Id = Guid.NewGuid(),
                        EmailSent = true
                    }
                });

            var model = new ApplicationMessagesViewModel
            {
                ApplicationReviewId = applicationReviewId,
                MessageText = "Test message",
                AdditionalActions = new() { Send = true }
            };

            var result = await _controller.ApplicationMessages(model);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.True(_controller.TempData.ContainsKey(
                ApplicationMessagesController.NotificationKeys.MessageSentBanner.ToString()));
        }

        // -----------------------------
        // File download tests
        // -----------------------------

        [Fact]
        public async Task ApplicationReviewMessageFileDownload_EmptyFileId_ReturnsBadRequest()
        {
            var result = await _controller.ApplicationReviewMessageFileDownload(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.Empty);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ApplicationReviewMessageFileDownload_NotShared_ReturnsForbid()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                    {
                        ApplicationId = applicationId,
                        SharedWithOfqual = true
                    }
                });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationMessageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationMessageByIdQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationMessageByIdQueryResponse
                    {
                        SharedWithOfqual = false
                    }
                });

            var result = await _controller.ApplicationReviewMessageFileDownload(
                applicationReviewId,
                messageId,
                fileId);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ApplicationReviewMessageFileDownload_ValidFile_ReturnsFile()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var messageId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                    {
                        ApplicationId = applicationId,
                        SharedWithOfqual = true
                    }
                });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationMessageByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationMessageByIdQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationMessageByIdQueryResponse
                    {
                        SharedWithOfqual = true
                    }
                });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetFileMetadataQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetFileMetadataQueryResponse>
                {
                    Success = true,
                    Value = new GetFileMetadataQueryResponse
                    {
                        Files = new()
                        {
                            new FileMetadataDto
                            {
                                FileId = fileId,
                                ApplicationId = applicationId,
                                MessageId = messageId,
                                FileName = "test.pdf",
                                BlobContainer = "files",
                                BlobPath = "path/blob",
                                ContentType = "application/pdf",
                                IsDownloadable = true
                            }
                        }
                    }
                });

            _fileServiceMock
                .Setup(f => f.OpenReadStreamAsync("files", "path/blob"))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("content")));

            var result = await _controller.ApplicationReviewMessageFileDownload(
                applicationReviewId,
                messageId,
                fileId);

            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal("test.pdf", fileResult.FileDownloadName);
        }
    }
}