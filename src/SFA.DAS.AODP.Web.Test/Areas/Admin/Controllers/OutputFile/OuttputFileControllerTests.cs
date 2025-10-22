using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers;
using SFA.DAS.AODP.Web.Models.OutputFile;
using static SFA.DAS.AODP.Application.Queries.Qualifications.GetQualificationOutputFileLogResponse;

namespace SFA.DAS.AODP.Web.Test.Controllers
{
    public class OutputFileControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<OutputFileController>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly OutputFileController _controller;

        // Shared constants for repeated strings
        private const string OutputFileErrorMessage = "Output file not available.";
        private const string DefaultContentType = "application/zip";
        private const string DefaultFileName = "export.zip";
        private const string TestUser = "import.user@example.com";
        private const string TestFileName = "qualifications_export.zip";

        public OutputFileControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _loggerMock = _fixture.Freeze<Mock<ILogger<OutputFileController>>>();
            _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
            _controller = new OutputFileController(_loggerMock.Object, _mediatorMock.Object);
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
            var result = await _controller.Index(CancellationToken.None);

            // Assert
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetQualificationOutputFileLogQuery>(), It.IsAny<CancellationToken>()), Times.Once);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OutputFileViewModel>(viewResult.Model);

            Assert.Multiple(() =>
            {
                Assert.NotNull(model.OutputFileLogs);
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
            var result = await _controller.Index(CancellationToken.None);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<OutputFileViewModel>(viewResult.Model);

            Assert.Multiple(() =>
            {
                Assert.NotNull(model.OutputFileLogs);
                Assert.Empty(model.OutputFileLogs);
            });
        }


        [Fact]
        public async Task StartDownload_ReturnsFile_WhenQuerySucceeds()
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
            var result = await _controller.StartDownload(CancellationToken.None);

            // Assert
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Multiple(() =>
            {
                Assert.Equal(DefaultContentType, file.ContentType);
                Assert.Equal(TestFileName, file.FileDownloadName);
                Assert.Equal(bytes, file.FileContents);
            });
        }

        [Fact]
        public async Task StartDownload_ReturnsProblem_WhenQueryFails()
        {
            // Arrange
            SetUserOnController(_controller, TestUser);

            var mediatorResponse = _fixture.Build<BaseMediatrResponse<GetQualificationOutputFileResponse>>()
                                           .With(r => r.Success, false)
                                           .With(r => r.ErrorMessage, OutputFileErrorMessage)
                                           .With(r => r.Value, (GetQualificationOutputFileResponse?)null)
                                           .Create();

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(mediatorResponse);

            // Act
            var result = await _controller.StartDownload(CancellationToken.None);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            var problem = Assert.IsType<ProblemDetails>(obj.Value);
            Assert.Multiple(() =>
            {
                Assert.Equal(500, obj.StatusCode);
                Assert.Equal(OutputFileErrorMessage, problem.Detail);
            });
        }

        [Fact]
        public async Task StartDownload_DefaultsContentTypeAndFileName_WhenMissing()
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
            var result = await _controller.StartDownload(CancellationToken.None);

            // Assert
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Multiple(() =>
            {
                Assert.Equal(DefaultContentType, file.ContentType);
                Assert.Equal(DefaultFileName, file.FileDownloadName);
                Assert.Equal(bytes, file.FileContents);
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
    }
}
