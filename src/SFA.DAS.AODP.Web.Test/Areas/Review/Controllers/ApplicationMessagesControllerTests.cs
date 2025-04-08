using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationMessage;
using SFA.DAS.AODP.Web.Helpers.File;
using SFA.DAS.AODP.Web.Helpers.User;
using System.Text;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers
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
            _controller = new(_mediatorMock.Object, _loggerMock.Object, _userHelperServiceMock.Object, _formBuilderSettings, _messageFileValidationService.Object, _fileService.Object);
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
                ApplicationReviewId = Guid.NewGuid(),
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

            var applicationSharingResponse = new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>()
            {
                Success = true,
                Value = new()
                {
                    ApplicationId = applicationId,
                }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateApplicationMessageCommand>(), default)).ReturnsAsync(msgResponse);
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationMetadataByIdQuery>(), default)).ReturnsAsync(metaDataResponse);
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), default)).ReturnsAsync(applicationSharingResponse);

            _controller.TempData = new Mock<ITempDataDictionary>().Object;

            // Act
            var result = await _controller.ApplicationMessages(model);

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


            var applicationSharingResponse = new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>()
            {
                Success = true,
                Value = new()
                {
                    ApplicationId = applicationId,
                }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), default)).ReturnsAsync(applicationSharingResponse);

            _messageFileValidationService.Setup(f => f.ValidateFiles(model.Files)).Throws(new Exception("error"));

            // Act
            var result = await _controller.ApplicationMessages(model);


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
            var file = $"messages/{applicationId}/";

            var blob = _fixture.Create<UploadedBlob>();
            string content = "Test file content";

            _fileService.Setup(f => f.GetBlobDetails(file)).ReturnsAsync(blob);
            _fileService.Setup(fs => fs.OpenReadStreamAsync(It.IsAny<string>()))
              .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)));


            var applicationSharingResponse = new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>()
            {
                Success = true,
                Value = new()
                {
                    ApplicationId = applicationId,
                }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), default)).ReturnsAsync(applicationSharingResponse);


            // Act
            var result = await _controller.ApplicationReviewMessageFileDownload(file, applicationId);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Equal(blob.FileNameWithPrefix, fileResult.FileDownloadName);

            using StreamReader reader = new(fileResult.FileStream);
            Assert.Equal(content, reader.ReadToEnd());
        }

        [Fact]
        public async Task ApplicationMessageFileDownload_InvalidFilePath_ReturnsBadRequest()
        {
            // Arrange
            var applicationId = _fixture.Create<Guid>();
            var file = $"messages/{Guid.NewGuid()}/";

            var applicationSharingResponse = new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>()
            {
                Success = true,
                Value = new()
                {
                    ApplicationId = applicationId,
                }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), default)).ReturnsAsync(applicationSharingResponse);


            // Act
            var result = await _controller.ApplicationReviewMessageFileDownload(file, applicationId);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }

}
