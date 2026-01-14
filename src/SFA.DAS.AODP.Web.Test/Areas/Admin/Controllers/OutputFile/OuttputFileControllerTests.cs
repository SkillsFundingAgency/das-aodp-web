using AutoFixture;
using AutoFixture.AutoMoq;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Infrastructure.Cache;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Models.OutputFile;
using System.Security.Claims;
using static SFA.DAS.AODP.Application.Queries.Qualifications.GetQualificationOutputFileLogResponse;

namespace SFA.DAS.AODP.Web.Test.Controllers
{
    public class OutputFileControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<OutputFileController>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly OutputFileController _controller;
        private readonly Mock<IValidator<OutputFileViewModel>> _validator;
        private readonly Mock<ICacheService> _cacheService;

        private const string ErrorMessageFromMediator = "Output file not available.";
        private const string DefaultContentType = "text/csv";
        private const string DefaultFileName = "export.csv";
        private const string TestUser = "import.user@example.com";
        private const string ControllerDefaultErrorMessage = OutputFileController.OutputFileDefaultErrorMessage;
        private OutputFileViewModel model = new OutputFileViewModel
        {
            DateChoice = PublicationDateMode.Today,
            Day = DateTime.UtcNow.Day,
            Month = DateTime.UtcNow.Month,
            Year = DateTime.UtcNow.Year
        };

        public OutputFileControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _loggerMock = _fixture.Freeze<Mock<ILogger<OutputFileController>>>();
            _mediatorMock = _fixture.Freeze<Mock<IMediator>>();
            _validator = _fixture.Create<Mock<IValidator<OutputFileViewModel>>>();
            _cacheService = _fixture.Create<Mock<ICacheService>>();

            _controller = new OutputFileController(_loggerMock.Object, _mediatorMock.Object, _validator.Object, _cacheService.Object);

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
                new() { UserDisplayName = "Bob",   DownloadDate = t0.AddMinutes(-1), PublicationDate = t0.AddDays(3)  },
                new() { UserDisplayName = "Alice", DownloadDate = t0.AddMinutes(-10), PublicationDate = t0.AddDays(4) },
                new() { UserDisplayName = "Carol", DownloadDate = t0.AddMinutes(-5), PublicationDate = t0.AddDays(5)  }
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
                        x.DownloadDate == expected.DownloadDate);

                    Assert.True(match, $"Expected log not found: {expected.UserDisplayName} @ {expected.DownloadDate}");
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
        public async Task CreateOutputFile_OnSuccess_CachesFile_SetsSuccessTempData_AndRedirectsToIndex()
        {
            // Arrange
            SetUserOnController(_controller, TestUser);
            SetTempData(_controller);

            var fileBytes = new byte[] { 1, 2, 3 };
            var payload = new GetQualificationOutputFileResponse
            {
                FileName = DefaultFileName,
                FileContent = fileBytes,
                ContentType = DefaultContentType
            };

            var mediatorResponse = new BaseMediatrResponse<GetQualificationOutputFileResponse>
            {
                Success = true,
                Value = payload
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResponse);

            // Capture cache call
            string? cachedKey = null;
            GetQualificationOutputFileResponse? cachedFile = null;

            _cacheService
                .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<GetQualificationOutputFileResponse>()))
                .Callback<string, GetQualificationOutputFileResponse>((key, value) =>
                {
                    cachedKey = key;
                    cachedFile = value;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateOutputFile(model);

            // Assert
            Assert.Multiple(() =>
            {
                // 1️⃣ Redirect back to Index
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(OutputFileController.Index), redirect.ActionName);

                // 2️⃣ Mediator called correctly
                _mediatorMock.Verify(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()), Times.Once);

                // 3️⃣ File cached with expected data
                Assert.NotNull(cachedKey);
                Assert.StartsWith("download:", cachedKey);
                Assert.NotNull(cachedFile);
                Assert.Equal(DefaultFileName, cachedFile!.FileName);
                Assert.Equal(DefaultContentType, cachedFile.ContentType);
                Assert.Equal(fileBytes, cachedFile.FileContent);

                // 4️⃣ TempData success keys set correctly
                Assert.True((bool)_controller.TempData[OutputFileTempDataKeys.Success]!);
                Assert.Equal(OutputFileController.OutputFileSuccessMessage,
                    _controller.TempData[OutputFileTempDataKeys.SuccessMessage]);
                Assert.NotNull(_controller.TempData[OutputFileTempDataKeys.SuccessToken]);
            });
        }

        [Fact]
        public async Task CreateOutputFile_WhenMediatorReturnsNoData_SetsFailureTempData_AndRedirectsToIndex()
        {
            // Arrange
            SetTempData(_controller);

            var mediatorResponse = new BaseMediatrResponse<GetQualificationOutputFileResponse>
            {
                Success = false,
                ErrorCode = ErrorCodes.NoData,
                ErrorMessage = "ignored"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResponse);

            // Act
            var result = await _controller.CreateOutputFile(model);

            // Assert
            Assert.Multiple(() =>
            {
                // Redirects to Index
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(OutputFileController.Index), redirect.ActionName);

                // Sets TempData failure flags
                Assert.True((bool)_controller.TempData[OutputFileTempDataKeys.Failed]!);
                Assert.Equal(OutputFileController.OutputFileNoDataErrorMessage,
                    _controller.TempData[OutputFileTempDataKeys.FailedMessage]);

                // No file cached on failure
                _cacheService.Verify(c =>
                    c.SetAsync(It.IsAny<string>(), It.IsAny<GetQualificationOutputFileResponse>()),
                    Times.Never);
            });

            // Mediator called exactly once
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateOutputFile_WhenMediatorReturnsUnexpectedError_SetsDefaultFailureTempData_AndRedirectsToIndex()
        {
            // Arrange
            SetTempData(_controller);

            var mediatorResponse = new BaseMediatrResponse<GetQualificationOutputFileResponse>
            {
                Success = false,
                ErrorCode = ErrorCodes.UnexpectedError,
                ErrorMessage = "Some server failure"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mediatorResponse);

            // Act
            var result = await _controller.CreateOutputFile(model);

            // Assert
            Assert.Multiple(() =>
            {
                // Redirects back to Index
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(OutputFileController.Index), redirect.ActionName);

                // Sets TempData failure flags with default error text
                Assert.True((bool)_controller.TempData[OutputFileTempDataKeys.Failed]!);
                Assert.Equal(OutputFileController.OutputFileDefaultErrorMessage,
                    _controller.TempData[OutputFileTempDataKeys.FailedMessage]);

                // No file cached
                _cacheService.Verify(c =>
                    c.SetAsync(It.IsAny<string>(), It.IsAny<GetQualificationOutputFileResponse>()),
                    Times.Never);
            });

            // Mediator called once
            _mediatorMock.Verify(m =>
                m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOutputFile_WhenModelValidationFails_ReturnsIndexView_WithValidationErrors()
        {
            // Arrange
            SetTempData(_controller);

            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new("DateChoice", "You must select a date option."),
                new("Day", "Day is required.")
            };

            var validationResult = new FluentValidation.Results.ValidationResult(validationFailures);

            _validator
                .Setup(v => v.ValidateAsync(It.IsAny<OutputFileViewModel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.CreateOutputFile(model);

            // Assert
            Assert.Multiple(() =>
            {
                // Returns Index view (not redirect)
                var view = Assert.IsType<ViewResult>(result);
                Assert.Equal("Index", view.ViewName);

                // ModelState contains expected errors
                Assert.False(_controller.ModelState.IsValid);
                Assert.True(_controller.ModelState.ContainsKey("DateChoice"));
                Assert.True(_controller.ModelState.ContainsKey("Day"));

                // Mediator not called
                _mediatorMock.Verify(m =>
                    m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()),
                    Times.Never);

                // No file cached
                _cacheService.Verify(c =>
                    c.SetAsync(It.IsAny<string>(), It.IsAny<GetQualificationOutputFileResponse>()),
                    Times.Never);
            });
        }

        [Fact]
        public async Task Download_WhenCacheMiss_ReturnsNotFound()
        {
            // Arrange
            const string token = "missingtoken123";
            _cacheService
                .Setup(c => c.GetAsync<GetQualificationOutputFileResponse>($"download:{token}"))
                .ReturnsAsync((GetQualificationOutputFileResponse?)null);

            // Act
            var result = await _controller.Download(token);

            // Assert
            Assert.Multiple(() =>
            {
                // Returns NotFound
                Assert.IsType<NotFoundResult>(result);

                // Cache not removed
                _cacheService.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);

                // Mediator not called
                _mediatorMock.Verify(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()), Times.Never);
            });
        }

        [Fact]
        public async Task Download_WhenCacheHit_ReturnsFile_AndRemovesFromCache()
        {
            // Arrange
            const string token = "validtoken456";

            var file = new GetQualificationOutputFileResponse
            {
                FileName = DefaultFileName,
                ContentType = DefaultContentType,
                FileContent = new byte[] { 1, 2, 3 }
            };

            _cacheService
                .Setup(c => c.GetAsync<GetQualificationOutputFileResponse>($"download:{token}"))
                .ReturnsAsync(file);

            // Act
            var result = await _controller.Download(token);

            // Assert
            Assert.Multiple(() =>
            {
                // Correct result type and headers
                var fileResult = Assert.IsType<FileContentResult>(result);
                Assert.Equal(DefaultContentType, fileResult.ContentType);
                Assert.Equal(DefaultFileName, fileResult.FileDownloadName);
                Assert.Equal(file.FileContent, fileResult.FileContents);

                // Cache cleaned up
                _cacheService.Verify(c => c.RemoveAsync($"download:{token}"), Times.Once);

                // Mediator untouched
                _mediatorMock.Verify(m => m.Send(It.IsAny<GetQualificationOutputFileQuery>(), It.IsAny<CancellationToken>()), Times.Never);
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
