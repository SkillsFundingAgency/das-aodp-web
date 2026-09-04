using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.Aodp.Domain.Files;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Review;
using SFA.DAS.AODP.Application.Commands.Application.Review;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Application.Queries.Review;
using SFA.DAS.AODP.Application.Queries.Users;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Helpers.Export;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Applications;
using SFA.DAS.AODP.Web.Validators.Messages;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace SFA.DAS.AODP.Web.Test.Areas.Review.Controllers
{
    public class ApplicationsReviewControllerTests
    {
        private readonly Fixture _fixture = new();

        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ILogger<ApplicationsReviewController>> _loggerMock = new();
        private readonly Mock<IUserHelperService> _userHelperServiceMock = new();
        private readonly Mock<IFileService> _fileServiceMock = new();
        private readonly Mock<IApplicationExportService> _applicationExportServiceMock = new();
        private readonly ApplicationsReviewController _controller;

        public ApplicationsReviewControllerTests()
        {
            _userHelperServiceMock.Setup(u => u.GetUserType()).Returns(UserType.Ofqual);
            _userHelperServiceMock.Setup(u => u.GetUserEmail()).Returns("user@test.com");
            _userHelperServiceMock.Setup(u => u.GetUserDisplayName()).Returns("Test User");

            _fixture.Register(() => DateOnly.FromDateTime(new DateTime(2020, 1, 1)));

            var options = Options.Create(new AodpConfiguration
            {
                FindRegulatedQualificationUrl =
                    "https://find-a-qualification.services.ofqual.gov.uk/qualifications/"
            });

            _controller = new(_loggerMock.Object, _mediatorMock.Object, _userHelperServiceMock.Object, _fileServiceMock.Object, options, _applicationExportServiceMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var urlMock = new Mock<IUrlHelper>();
            urlMock.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("/fake-url");
            urlMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/fake-url");
            _controller.Url = urlMock.Object;

            _controller.TempData = new TempDataDictionary(
                _controller.HttpContext,
                Mock.Of<ITempDataProvider>());
        }

        // -----------------------------
        // Index
        // -----------------------------

        [Fact]
        public async Task Index_ReturnsView_WithApplications()
        {
            var query = new ApplicationsReviewQuery();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationsForReviewQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationsForReviewQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationsForReviewQueryResponse
                    {
                        TotalRecordsCount = 1,
                        Applications = new()
                    }
                });

            var result = await _controller.Index(query);

            Assert.IsType<ViewResult>(result);
        }

        // -----------------------------
        // Single file download
        // -----------------------------

        [Fact]
        public async Task ApplicationFileDownload_ValidFile_ReturnsFile()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var fileId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
                {
                    Success = true,
                    Value = new()
                    {
                        ApplicationId = applicationId,
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
                                FileName = "file.pdf",
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

            var result = await _controller.ApplicationFileDownload(
                new ApplicationFileDownloadViewModel
                {
                    ApplicationReviewId = applicationReviewId,
                    FileId = fileId
                });

            var file = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/pdf", file.ContentType);
        }

        // -----------------------------
        // ZIP download
        // -----------------------------

        [Fact]
        public async Task DownloadAllApplicationFiles_ReturnsZip()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
                {
                    Success = true,
                    Value = new()
                    {
                        ApplicationId = applicationId,
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
                                ApplicationId = applicationId,
                                FileId = Guid.NewGuid(),
                                BlobContainer = "files",
                                BlobPath = "file1",
                                FileName = "file1.txt",
                                IsDownloadable = true
                            }
                        }
                    }
                });

            _fileServiceMock
                .Setup(f => f.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes("test")));

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationMetadataByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationMetadataByIdQueryResponse
                    {
                        Reference = 123456
                    }
                });

            var result = await _controller.DownloadAllApplicationFiles(applicationReviewId);

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/zip", file.ContentType);

            using var zip = new ZipArchive(new MemoryStream(file.FileContents));
            Assert.Single(zip.Entries);
        }

        // -----------------------------
        // UpdateReviewer
        // -----------------------------

        [Fact]
        public async Task UpdateReviewer_NoDuplicate_Redirects()
        {
            var model = new UpdateReviewerViewModel
            {
                ApplicationReviewId = Guid.NewGuid(),
                ApplicationId = Guid.NewGuid(),
                ReviewerFieldName = nameof(ApplicationReviewViewModel.Reviewer1),
                ReviewerValue = "Reviewer Name"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new BaseMediatrResponse<SaveReviewerCommandResponse>
                    {
                        Success = true,
                        Value = new SaveReviewerCommandResponse
                        {
                            DuplicateReviewerError = false
                        }
                    }));

            var result = await _controller.UpdateReviewer(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), redirect.ActionName);
            Assert.True(_controller.TempData.ContainsKey("ReviewerUpdated"));
        }

        [Fact]
        public async Task SaveApplicationDetails_InvalidWebQanValidation_SendsNoUpdateCommands()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId);

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);

            var model = CreateSaveApplicationDetailsViewModel(applicationReviewId, applicationId, qan: "bad-qan");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), viewResult.ViewName);
                Assert.False(_controller.ModelState.IsValid);
                Assert.True(_controller.ModelState.ContainsKey(nameof(SaveApplicationDetailsViewModel.Qan)));
                VerifyNoApplicationDetailUpdateCommands();
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_DownstreamInvalidQan_SendsNoReviewerCommands()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, qan: "12345678");

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);
            SetupSaveQan(isValid: false, validationMessage: "Downstream QAN validation failed");

            var model = CreateSaveApplicationDetailsViewModel(applicationReviewId, applicationId, qan: "1234567X");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), viewResult.ViewName);
                Assert.False(_controller.ModelState.IsValid);
                Assert.Contains("Downstream QAN validation failed", _controller.ModelState[nameof(SaveApplicationDetailsViewModel.Qan)]!.Errors.Select(e => e.ErrorMessage));
                _mediatorMock.Verify(m => m.Send(It.IsAny<SaveQanCommand>(), It.IsAny<CancellationToken>()), Times.Once);
                _mediatorMock.Verify(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()), Times.Never);
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_DuplicateSubmittedReviewers_SendsNoUpdateCommands()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId);

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);

            var model = CreateSaveApplicationDetailsViewModel(
                applicationReviewId,
                applicationId,
                reviewer1: "Alice Smith",
                reviewer2: "Alice Smith");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), viewResult.ViewName);
                Assert.False(_controller.ModelState.IsValid);
                Assert.Contains(ValidationMessages.Reviewer1Reviewer2Conflict, _controller.ModelState[nameof(SaveApplicationDetailsViewModel.Reviewer1)]!.Errors.Select(e => e.ErrorMessage));
                VerifyNoApplicationDetailUpdateCommands();
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_OnlyQanChanged_SendsOnlyQanCommand()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, qan: "12345678");

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);
            SetupSaveQan();

            var model = CreateSaveApplicationDetailsViewModel(applicationReviewId, applicationId, qan: "1234567X");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), redirect.ActionName);
                Assert.True(_controller.TempData.ContainsKey("ApplicationDetailsUpdated"));
                Assert.False(_controller.TempData.ContainsKey("QanUpdated"));
                Assert.False(_controller.TempData.ContainsKey("ReviewerUpdated"));
                _mediatorMock.Verify(m => m.Send(It.Is<SaveQanCommand>(c =>
                    c.ApplicationReviewId == applicationReviewId &&
                    c.Qan == "1234567X" &&
                    c.SentByEmail == "user@test.com" &&
                    c.SentByName == "Test User"), It.IsAny<CancellationToken>()), Times.Once);
                _mediatorMock.Verify(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()), Times.Never);
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_OnlyReviewer1Changed_SendsOnlyReviewer1Command()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, reviewer1: "Alice Smith", reviewer2: "Bob Jones");

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);
            SetupSaveReviewer();

            var model = CreateSaveApplicationDetailsViewModel(
                applicationReviewId,
                applicationId,
                reviewer1: "Charlie Brown",
                reviewer2: "Bob Jones");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), redirect.ActionName);
                _mediatorMock.Verify(m => m.Send(It.IsAny<SaveQanCommand>(), It.IsAny<CancellationToken>()), Times.Never);
                _mediatorMock.Verify(m => m.Send(It.Is<SaveReviewerCommand>(c =>
                    c.ApplicationId == applicationId &&
                    c.ReviewerFieldName == nameof(ApplicationReviewViewModel.Reviewer1) &&
                    c.ReviewerValue == "Charlie Brown" &&
                    c.UserType == UserType.Qfau.ToString() &&
                    c.SentByEmail == "user@test.com" &&
                    c.SentByName == "Test User"), It.IsAny<CancellationToken>()), Times.Once);
                _mediatorMock.Verify(m => m.Send(It.Is<SaveReviewerCommand>(c =>
                    c.ReviewerFieldName == nameof(ApplicationReviewViewModel.Reviewer2)), It.IsAny<CancellationToken>()), Times.Never);
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_AllDetailsChanged_SendsQanAndBothReviewerCommands()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, qan: "12345678", reviewer1: "Alice Smith", reviewer2: "Bob Jones");

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);
            SetupSaveQan();
            SetupSaveReviewer();

            var model = CreateSaveApplicationDetailsViewModel(
                applicationReviewId,
                applicationId,
                qan: "1234567X",
                reviewer1: "Charlie Brown",
                reviewer2: "Dana White");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                Assert.IsType<RedirectToActionResult>(result);
                _mediatorMock.Verify(m => m.Send(It.IsAny<SaveQanCommand>(), It.IsAny<CancellationToken>()), Times.Once);
                _mediatorMock.Verify(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_UnchangedValues_SendsNoUpdateCommands()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId);

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);

            var model = CreateSaveApplicationDetailsViewModel(applicationReviewId, applicationId);

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                Assert.IsType<RedirectToActionResult>(result);
                Assert.True(_controller.TempData.ContainsKey("ApplicationDetailsUpdated"));
                VerifyNoApplicationDetailUpdateCommands();
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_ReviewerOrderAvoidsTemporaryDuplicate()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, reviewer1: "Alice Smith", reviewer2: "Bob Jones");
            var commandOrder = new List<string>();

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);
            SetupSaveReviewer(command => commandOrder.Add(command.ReviewerFieldName));

            var model = CreateSaveApplicationDetailsViewModel(
                applicationReviewId,
                applicationId,
                reviewer1: "Bob Jones",
                reviewer2: "Charlie Brown");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(new[] { nameof(ApplicationReviewViewModel.Reviewer2), nameof(ApplicationReviewViewModel.Reviewer1) }, commandOrder);
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_DirectReviewerSwap_ReturnsValidationError()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, reviewer1: "Alice Smith", reviewer2: "Bob Jones");

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);

            var model = CreateSaveApplicationDetailsViewModel(
                applicationReviewId,
                applicationId,
                reviewer1: "Bob Jones",
                reviewer2: "Alice Smith");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), viewResult.ViewName);
                Assert.False(_controller.ModelState.IsValid);
                Assert.True(_controller.ModelState.ContainsKey(nameof(SaveApplicationDetailsViewModel.Reviewer1)));
                VerifyNoApplicationDetailUpdateCommands();
            });
        }

        [Theory]
        [InlineData("")]
        [InlineData(ReviewerDropdown.UnassignedValue)]
        public async Task SaveApplicationDetails_BlankOrUnassignedReviewer_IsNormalisedToNull(string reviewerValue)
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, reviewer1: "Alice Smith", reviewer2: "Bob Jones");

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);
            SetupSaveReviewer();

            var model = CreateSaveApplicationDetailsViewModel(
                applicationReviewId,
                applicationId,
                reviewer1: reviewerValue,
                reviewer2: "Bob Jones");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                Assert.IsType<RedirectToActionResult>(result);
                _mediatorMock.Verify(m => m.Send(It.Is<SaveReviewerCommand>(c =>
                    c.ReviewerFieldName == nameof(ApplicationReviewViewModel.Reviewer1) &&
                    c.ReviewerValue == null), It.IsAny<CancellationToken>()), Times.Once);
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_WhenApplicationWithdrawn_ReturnsBadRequest()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, status: ApplicationStatus.Withdrawn);

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);

            var model = CreateSaveApplicationDetailsViewModel(applicationReviewId, applicationId, qan: "1234567X");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                Assert.IsType<BadRequestResult>(result);
                VerifyNoApplicationDetailUpdateCommands();
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_WhenApplicationCompleted_ReturnsBadRequest()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, status: ApplicationStatus.Approved, sharedWithOfqual: true);

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);

            var model = CreateSaveApplicationDetailsViewModel(applicationReviewId, applicationId, qan: "1234567X");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                Assert.IsType<BadRequestResult>(result);
                VerifyNoApplicationDetailUpdateCommands();
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_WhenUserIsNotQfau_ReturnsForbid()
        {
            _userHelperServiceMock.Setup(x => x.GetUserType()).Returns(UserType.Ofqual);

            var result = await _controller.SaveApplicationDetails(CreateSaveApplicationDetailsViewModel(Guid.NewGuid(), Guid.NewGuid()));

            Assert.Multiple(() =>
            {
                Assert.IsType<ForbidResult>(result);
                _mediatorMock.Verify(m => m.Send(It.IsAny<GetApplicationForReviewByIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
                VerifyNoApplicationDetailUpdateCommands();
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_ValidationFailure_PreservesSubmittedValuesAndReviewerOptions()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, reviewer1: "Alice Smith", reviewer2: "Bob Jones");

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);

            var model = CreateSaveApplicationDetailsViewModel(
                applicationReviewId,
                applicationId,
                qan: "1234567X",
                reviewer1: "Charlie Brown",
                reviewer2: "Charlie Brown");

            var result = await _controller.SaveApplicationDetails(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<ApplicationReviewViewModel>(viewResult.Model);

            Assert.Multiple(() =>
            {
                Assert.Equal("1234567X", viewModel.Qan);
                Assert.Equal("Charlie Brown", viewModel.Reviewer1);
                Assert.Equal("Charlie Brown", viewModel.Reviewer2);
                Assert.Contains(viewModel.ReviewerOptions, option => option.Value == "Charlie Brown");
                Assert.Contains(viewModel.ReviewerOptions, option => option.Value == ReviewerDropdown.UnassignedValue);
            });
        }

        [Fact]
        public async Task SaveApplicationDetails_LaterCommandFailure_RedirectsWithFailureAndNoSuccess()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var review = CreateReview(applicationReviewId, applicationId, qan: "12345678", reviewer1: "Alice Smith", reviewer2: "Bob Jones");

            SetupQfauUser();
            SetupControllerContext();
            SetupReviewResponse(review);
            SetupSaveQan();
            _mediatorMock.Setup(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<SaveReviewerCommandResponse>
                {
                    Success = false,
                    ErrorMessage = "Reviewer update failed",
                    Value = new SaveReviewerCommandResponse()
                });

            var model = CreateSaveApplicationDetailsViewModel(
                applicationReviewId,
                applicationId,
                qan: "1234567X",
                reviewer1: "Charlie Brown",
                reviewer2: "Bob Jones");

            var result = await _controller.SaveApplicationDetails(model);

            Assert.Multiple(() =>
            {
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), redirect.ActionName);
                Assert.True(_controller.TempData.ContainsKey("ApplicationDetailsUpdateFailed"));
                Assert.False(_controller.TempData.ContainsKey("ApplicationDetailsUpdated"));
            });
        }

        private void SetupControllerContext()
        {
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        private void SetupQfauUser()
        {
            _userHelperServiceMock.Setup(x => x.GetUserType()).Returns(UserType.Qfau);
            _userHelperServiceMock.Setup(x => x.GetUserEmail()).Returns("user@test.com");
            _userHelperServiceMock.Setup(x => x.GetUserDisplayName()).Returns("Test User");
        }

        private void SetupReviewResponse(GetApplicationForReviewByIdQueryResponse review)
        {
            _mediatorMock.Setup(m => m.Send(It.Is<GetApplicationForReviewByIdQuery>(q =>
                    q.ApplicationReviewId == review.ApplicationReviewId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationForReviewByIdQueryResponse>
                {
                    Success = true,
                    Value = review
                });
        }

        private void SetupSaveQan(bool? isValid = true, string? validationMessage = null)
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<SaveQanCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<SaveQanCommandResponse>
                {
                    Success = true,
                    Value = new SaveQanCommandResponse
                    {
                        IsQanValid = isValid,
                        QanValidationMessage = validationMessage
                    }
                });
        }

        private void SetupSaveReviewer(Action<SaveReviewerCommand>? callback = null)
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<BaseMediatrResponse<SaveReviewerCommandResponse>>, CancellationToken>((command, _) =>
                    callback?.Invoke((SaveReviewerCommand)command))
                .ReturnsAsync(new BaseMediatrResponse<SaveReviewerCommandResponse>
                {
                    Success = true,
                    Value = new SaveReviewerCommandResponse
                    {
                        DuplicateReviewerError = false
                    }
                });
        }

        private void VerifyNoApplicationDetailUpdateCommands()
        {
            _mediatorMock.Verify(m => m.Send(It.IsAny<SaveQanCommand>(), It.IsAny<CancellationToken>()), Times.Never);
            _mediatorMock.Verify(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static SaveApplicationDetailsViewModel CreateSaveApplicationDetailsViewModel(
            Guid applicationReviewId,
            Guid applicationId,
            string? qan = "12345678",
            string? reviewer1 = "Alice Smith",
            string? reviewer2 = "Bob Jones")
        {
            return new SaveApplicationDetailsViewModel
            {
                ApplicationReviewId = applicationReviewId,
                ApplicationId = applicationId,
                Qan = qan,
                Reviewer1 = reviewer1,
                Reviewer2 = reviewer2
            };
        }

        private static GetApplicationForReviewByIdQueryResponse CreateReview(
            Guid applicationReviewId,
            Guid applicationId,
            string? qan = "12345678",
            string? reviewer1 = "Alice Smith",
            string? reviewer2 = "Bob Jones",
            ApplicationStatus status = ApplicationStatus.InReview,
            bool sharedWithOfqual = false)
        {
            return new GetApplicationForReviewByIdQueryResponse
            {
                Id = applicationId,
                ApplicationReviewId = applicationReviewId,
                Name = "Test application",
                LastUpdated = DateTime.UtcNow,
                Reference = 123456,
                Qan = qan,
                AwardingOrganisation = "Test awarding organisation",
                SharedWithSkillsEngland = false,
                SharedWithOfqual = sharedWithOfqual,
                FormTitle = "Test form",
                ApplicationStatus = status.ToString(),
                Reviewer1 = reviewer1,
                Reviewer2 = reviewer2,
                Feedbacks = new List<GetApplicationForReviewByIdQueryResponse.Feedback>
                {
                    new()
                    {
                        Owner = "Owner",
                        Status = ApplicationStatus.InReview.ToString(),
                        NewMessage = false,
                        UserType = UserType.Qfau.ToString(),
                        LatestCommunicatedToAwardingOrganisation = false
                    }
                },
                AvailableReviewers = new List<UserOption>
                {
                    CreateUserOption("Alice", "Smith"),
                    CreateUserOption("Bob", "Jones"),
                    CreateUserOption("Charlie", "Brown"),
                    CreateUserOption("Dana", "White")
                }
            };
        }

        private static UserOption CreateUserOption(string firstName, string lastName)
        {
            return new UserOption
            {
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName}.{lastName}@test.com"
            };
        }

    }
}