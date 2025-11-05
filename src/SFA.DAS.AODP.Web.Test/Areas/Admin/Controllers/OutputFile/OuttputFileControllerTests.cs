using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers;
using SFA.DAS.AODP.Web.Models.OutputFile;
using static SFA.DAS.AODP.Application.Queries.Qualifications.GetQualificationOutputFileLogResponse;
using SFA.DAS.AODP.Web.Enums;

namespace SFA.DAS.AODP.Web.Test.Controllers
{
    public class OutputFileControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<OutputFileController>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly OutputFileController _controller;

        private const string ErrorMessageFromMediator = "Output file not available.";
        private const string DefaultContentType = "application/zip";
        private const string DefaultFileName = "export.zip";
        private const string TestUser = "import.user@example.com";
        private const string TestFileName = "qualifications_export.zip";
        private const string TempDataKeyFailed = OutputFileController.OutputFileFailed;
        private const string TempDataKeyFailedMessage = OutputFileController.OutputFileFailed + ":Message";
        private const string ControllerDefaultErrorMessage = OutputFileController.OutputFileDefaultErrorMessage;

        public OutputFileControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _loggerMock = _fixture.Freeze<Mock<ILogger<OutputFileController>>>();
            _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
            _controller = new OutputFileController(_loggerMock.Object, _mediatorMock.Object);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithMappedLogs()
        {
            var t0 = DateTime.UtcNow;
            var logsIn = new List<QualificationOutputFileLog>
            {
                new() { UserDisplayName = "Bob",   Timestamp = t0.AddMinutes(-1)  },
                new() { UserDisplayName = "Alice", Timestamp = t0.AddMinutes(-10) },
                new() { UserDisplayName = "Carol", Timestamp = t0.AddMinutes(-5)  }
            };

            var payload = new GetQualificationOutputFileLogResponse { OutputFileLogs = logsIn };
            var envelope = new BaseMediatrResponse<GetQualificationOutputFileLogResponse>
            {
                Success = true,
                Value = payload
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetQualificationOutputFileLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.Multiple(() =>
            {
                _mediatorMock.Verify(m => m.Send(It.IsAny<GetQualificationOutputFileLogQuery>(), It.IsAny<CancellationToken>()), Times.Once);

                var viewResult = result as ViewResult;
                Assert.NotNull(viewResult);

                var model = viewResult!.Model as OutputFileViewModel;
                Assert.NotNull(model);

                Assert.NotNull(model!.OutputFileLogs);
                Assert.Equal(logsIn.Count, model.OutputFileLogs.Count);

                foreach (var expected in logsIn)
                {
                    var match = model.OutputFileLogs.Any(x =>
                        x.UserDisplayName == expected.UserDisplayName &&
                        x.Timestamp == expected.Timestamp);

                    Assert.True(match, $"Expected log not found: {expected.UserDisplayName} @ {expected.Timestamp}");
                }
            });
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithEmptyList_WhenNoLogs()
        {
            // Arrange
            var envelope = new BaseMediatrResponse<GetQualificationOutputFileLogResponse>
            {
                Success = false,
                ErrorMessage = "No output file logs available.",
                Value = new GetQualificationOutputFileLogResponse()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetQualificationOutputFileLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.Multiple(() =>
            {
                var viewResult = result as ViewResult;
                Assert.NotNull(viewResult);

                var model = viewResult!.Model as OutputFileViewModel;
                Assert.NotNull(model);

                Assert.NotNull(model!.OutputFileLogs);
                Assert.Empty(model.OutputFileLogs);
            });
        }

        [Fact]
        public async Task GetOutputFile_ReturnsFile_WhenQuerySucceeds()
        {
            // Arrange
            SetUserOnController(_controller, TestUser);

            var bytes = new byte[] { 1, 2, 3 };
            var payload = _fixture.Build<GetQualificationOutputFileResponse>()
                                  .With(p => p.FileName, TestFileName)
                                  .With(p => p.ZipFileContent, bytes)
                                  .With(p => p.ContentType, DefaultContentType)
                                  .Create();

            var mediatorResponse = _fixture.Build<BaseMediatrResponse<GetQualificationOutputFileResponse>>()
                                           .With(r => r.Success, true)
                                           .With(r => r.Value, payload)
                                           .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(mediatorResponse);

            // Act
            var result = await _controller.GetOutputFile();

            // Assert
            Assert.Multiple(() =>
            {
                var file = result as FileContentResult;
                Assert.NotNull(file);
                Assert.Equal(DefaultContentType, file!.ContentType);
                Assert.Equal(TestFileName, file.FileDownloadName);
                Assert.Equal(bytes, file.FileContents);
            });
        }

        [Fact]
        public async Task GetOutputFile_Failure_SetsTempData_AndRedirectsToIndex()
        {
            // Arrange
            SetTempData(_controller);

            var mediatorResponse = _fixture.Build<BaseMediatrResponse<GetQualificationOutputFileResponse>>()
                                           .With(r => r.Success, false)
                                           .With(r => r.ErrorMessage, ErrorMessageFromMediator)
                                           .With(r => r.ErrorCode, ErrorCodes.UnexpectedError)
                                           .With(r => r.Value, (GetQualificationOutputFileResponse?)null)
                                           .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(mediatorResponse);

            // Act
            var result = await _controller.GetOutputFile();

            // Assert
            Assert.Multiple(() =>
            {
                var redirect = result as RedirectToActionResult;
                Assert.NotNull(redirect);
                Assert.Equal(nameof(OutputFileController.Index), redirect!.ActionName);

                Assert.True((bool)_controller.TempData[TempDataKeyFailed]!);
                Assert.Equal(OutputFileController.OutputFileDefaultErrorMessage, _controller.TempData[TempDataKeyFailedMessage]);
            });
        }

        [Fact]
        public async Task GetOutputFile_Failure_WithNullMessage_UsesDefaultText_AndRedirects()
        {
            // Arrange
            SetTempData(_controller);

            var mediatorResponse = _fixture.Build<BaseMediatrResponse<GetQualificationOutputFileResponse>>()
                                           .With(r => r.Success, false)
                                           .With(r => r.ErrorMessage, (string?)null)
                                           .With(r => r.ErrorCode, (string?)null)
                                           .With(r => r.Value, (GetQualificationOutputFileResponse?)null)
                                           .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(mediatorResponse);

            // Act
            var result = await _controller.GetOutputFile();

            // Assert
            Assert.Multiple(() =>
            {
                var redirect = result as RedirectToActionResult;
                Assert.NotNull(redirect);
                Assert.Equal(nameof(OutputFileController.Index), redirect!.ActionName);
                Assert.True((bool)_controller.TempData[TempDataKeyFailed]!);
                Assert.Equal(OutputFileController.OutputFileDefaultErrorMessage, _controller.TempData[TempDataKeyFailedMessage]);
            });
        }

        [Fact]
        public async Task GetOutputFile_DefaultsContentTypeAndFileName_WhenMissing()
        {
            // Arrange
            SetUserOnController(_controller, TestUser);

            var bytes = new byte[] { 9, 9, 9 };
            var payload = _fixture.Build<GetQualificationOutputFileResponse>()
                                  .With(p => p.FileName, " ")
                                  .With(p => p.ZipFileContent, bytes)
                                  .With(p => p.ContentType, string.Empty)
                                  .Create();

            var mediatorResponse = new BaseMediatrResponse<GetQualificationOutputFileResponse>
            {
                Success = true,
                Value = payload
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(mediatorResponse);

            // Act
            var result = await _controller.GetOutputFile();

            // Assert
            Assert.Multiple(() =>
            {
                var file = result as FileContentResult;
                Assert.NotNull(file);

                Assert.Equal(DefaultContentType, file!.ContentType);
                Assert.Equal(DefaultFileName, file.FileDownloadName);
                Assert.Equal(bytes, file.FileContents);
            });
        }

        [Fact]
        public async Task GetOutputFile_NoDataErrorCode_SetsNoDataMessageAndRedirects()
        {
            // Arrange
            SetTempData(_controller);

            var mediatorResponse = new BaseMediatrResponse<GetQualificationOutputFileResponse>
            {
                Success = false,
                ErrorCode = ErrorCodes.NoData,
                ErrorMessage = "ignored",
                Value = new GetQualificationOutputFileResponse()
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(mediatorResponse);

            // Act
            var result = await _controller.GetOutputFile();

            // Assert
            Assert.Multiple(() =>
            {
                var redirect = result as RedirectToActionResult;
                Assert.NotNull(redirect);
                Assert.Equal(nameof(OutputFileController.Index), redirect!.ActionName);

                Assert.True((bool)_controller.TempData[OutputFileController.OutputFileFailed]!);
                Assert.Equal(OutputFileController.OutputFileNoDataErrorMessage, _controller.TempData[$"{OutputFileController.OutputFileFailed}:Message"]);
            });
        }

        private static void SetUserOnController(ControllerBase controller, string username)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, username) }, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private static void SetTempData(Controller controller)
        {
            controller.TempData = new TempDataDictionary(
                controller.HttpContext ?? new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
        }
    }
}
