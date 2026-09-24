using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Commands.Application.Review;
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
using SFA.DAS.AODP.Web.Validators.Messages;
using System.IO.Compression;

namespace SFA.DAS.AODP.Web.Test.Areas.Review.Controllers
{
    public class ApplicationsReviewControllerTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<ILogger<ApplicationsReviewController>> _loggerMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<IUserHelperService> _userHelperServiceMock = new();
        private readonly Mock<IFileService> _fileServiceMock = new();
        private readonly Mock<IApplicationExportService> _applicationExportServiceMock = new();
        private readonly ApplicationsReviewController _controller;
        private readonly IOptions<AodpConfiguration> _aodpOptions = Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/"
        });

        public ApplicationsReviewControllerTests()
        {
            _fixture.Register(() => DateOnly.FromDateTime(new DateTime(2020, 1, 1)));
            _controller = new(_loggerMock.Object, _mediatorMock.Object, _userHelperServiceMock.Object, _fileServiceMock.Object, _aodpOptions, _applicationExportServiceMock.Object);
        }

        [Fact]
        public async Task IndexMethod_PopulatesAndReturnsViewCorrectly()
        {
            //Arrange
            var expectedUserType = UserType.Ofqual;
            _userHelperServiceMock.Setup(x => x.GetUserType()).Returns(expectedUserType);

            var expectedModel = new Models.Applications.ApplicationsReviewQuery
            {
                PageNumber = 2,
                RecordsPerPage = 10,
                ApplicationSearch = "Test Search",
                AwardingOrganisationSearch = "Test AO",
                Status = new List<ApplicationStatus> { ApplicationStatus.InReview, ApplicationStatus.Approved },
                ReviewerSelection = "Bob Smith"
            };

            var expectedApplication = new GetApplicationsForReviewQueryResponse.Application
            {
                Id = Guid.NewGuid(),
                ApplicationReviewId = Guid.NewGuid(),
                Name = "TestApp",
                LastUpdated = DateTime.UtcNow,
                Reference = 123456,
                Qan = "123456",
                AwardingOrganisation = "Test Org",
                Owner = "TestOwner",
                Status = ApplicationStatus.InReview,
                NewMessage = false,
                FindRegulatedQualificationUrl = _aodpOptions.Value.FindRegulatedQualificationUrl,
                Reviewer1 = "Bob Smith",
                Reviewer2 = "Alice Jones"
            };

            var response = new GetApplicationsForReviewQueryResponse
            {
                TotalRecordsCount = 1,
                Applications = new List<GetApplicationsForReviewQueryResponse.Application> { expectedApplication }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationsForReviewQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationsForReviewQueryResponse> { Success = true, Value = response});

            //Act
            var result = await _controller.Index(expectedModel) as ViewResult;

            //Assert
            var model = Assert.IsType<ApplicationsReviewListViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(expectedUserType.ToString(), model.UserType);
            // Check the url is set correctly from configuration
            Assert.Equal(_aodpOptions.Value.FindRegulatedQualificationUrl, model.FindRegulatedQualificationUrl);
            Assert.Equal(response.TotalRecordsCount, model.TotalItems);

            var app = Assert.Single(model.Applications);
            Assert.Equal(expectedApplication.Id, app.Id);
            Assert.Equal(expectedApplication.Name, app.Name);
            Assert.Equal(expectedApplication.Reference, app.Reference);
            Assert.Equal(expectedApplication.Qan, app.Qan);
            Assert.Contains("Bob Smith", app.ReviewersSummary);
            Assert.Contains("Alice Jones", app.ReviewersSummary);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetApplicationsForReviewQuery>(q =>
                    q.ReviewUser == expectedUserType.ToString() &&
                    q.ApplicationSearch == expectedModel.ApplicationSearch &&
                    q.AwardingOrganisationSearch == expectedModel.AwardingOrganisationSearch &&
                    q.ApplicationStatuses.SequenceEqual(expectedModel.Status.Select(s => s.ToString())) &&
                    q.ApplicationsWithNewMessages == expectedModel.Status.Contains(ApplicationStatus.NewMessage) &&
                    q.Limit == expectedModel.RecordsPerPage &&
                    q.Offset == expectedModel.RecordsPerPage * (expectedModel.PageNumber - 1) &&
                    q.ReviewerSearch == "Bob Smith" &&
                    q.UnassignedOnly == false),
                It.IsAny<CancellationToken>()),
                Times.Once);

        }

        [Fact]
        public async Task DownloadAllApplicationFiles_Success_ReturnsZipFile()
        {
            // Arrange
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var files = new List<UploadedBlob>
            {
                new UploadedBlob { FullPath = "file1.txt", FileName = "file1.txt", Extension=".txt", FileNamePrefix="Q1" },
                new UploadedBlob { FullPath = "file2.txt", FileName = "file2.txt", Extension=".txt" },
            };
            var sharingResponse = new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
            {
                Success = true,
                Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                {
                    ApplicationId = applicationId,
                    SharedWithOfqual = true,
                    SharedWithSkillsEngland = true
                }
            };
            var applicationMetadata = new BaseMediatrResponse<GetApplicationMetadataByIdQueryResponse>
            {
                Success = true,
                Value = new GetApplicationMetadataByIdQueryResponse { Reference = 12345 }
            };

            _fileServiceMock.Setup(fs => fs.ListBlobs(applicationId.ToString())).Returns(files);
            _fileServiceMock.Setup(fs => fs.OpenReadStreamAsync(It.IsAny<string>()))
                .ReturnsAsync(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test file content")));

            _mediatorMock.Setup(m => m.Send(It.Is<GetApplicationReviewSharingStatusByIdQuery>(query => query.ApplicationReviewId == applicationReviewId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sharingResponse);
            _mediatorMock.Setup(m => m.Send(It.Is<GetApplicationMetadataByIdQuery>(query => query.ApplicationId == applicationId), It.IsAny<CancellationToken>()))
               .ReturnsAsync(applicationMetadata);

            // Act
            var result = await _controller.DownloadAllApplicationFiles(applicationReviewId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/zip", fileResult.ContentType);
            Assert.StartsWith(applicationMetadata.Value.Reference.ToString(), fileResult.FileDownloadName);
            Assert.EndsWith("-allfiles.zip", fileResult.FileDownloadName);

            // Verify ZIP content (optional)
            using (var memoryStream = new MemoryStream(fileResult.FileContents))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                Assert.Equal(2, archive.Entries.Count);
                Assert.Contains("Q1 file1.txt", archive.Entries.Select(e => e.FullName));
                Assert.Contains("file2.txt", archive.Entries.Select(e => e.FullName));
            }
        }

        [Fact]
        public async Task DownloadAllApplicationFiles_NoFiles_ThrowsInvalidOperationException()
        {
            // Arrange
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var sharingResponse = new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
            {
                Success = true,
                Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                {
                    ApplicationId = applicationId,
                    SharedWithOfqual = true,
                    SharedWithSkillsEngland = true
                }
            };

            _mediatorMock.Setup(m => m.Send(It.Is<GetApplicationReviewSharingStatusByIdQuery>(query => query.ApplicationReviewId == applicationReviewId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sharingResponse);
            _fileServiceMock.Setup(fs => fs.ListBlobs(applicationId.ToString())).Returns(new List<UploadedBlob>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DownloadAllApplicationFiles(applicationReviewId));

        }

        [Fact]
        public async Task DownloadAllApplicationFiles_FileServiceError_ThrowsException()
        {
            // Arrange
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var files = new List<UploadedBlob> { new UploadedBlob { FullPath = "file1.txt", FileName = "file1.txt", FileNamePrefix = "Q1" } };
            var sharingResponse = new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
            {
                Success = true,
                Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                {
                    ApplicationId = applicationId,
                    SharedWithOfqual = true,
                    SharedWithSkillsEngland = true
                }
            };


            _fileServiceMock.Setup(fs => fs.ListBlobs(applicationId.ToString())).Returns(files);
            _fileServiceMock.Setup(fs => fs.OpenReadStreamAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("File service error"));

            _mediatorMock.Setup(m => m.Send(It.Is<GetApplicationReviewSharingStatusByIdQuery>(query => query.ApplicationReviewId == applicationReviewId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sharingResponse);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DownloadAllApplicationFiles(applicationReviewId));
        }

        [Fact]
        public async Task DownloadAllApplicationFiles_FileStreamIsNull_ThrowsIOException()
        {
            // Arrange
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var files = new List<UploadedBlob> { new UploadedBlob { FullPath = "file1.txt", FileName = "file1.txt", FileNamePrefix = "Q1" } };
            var sharingResponse = new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
            {
                Success = true,
                Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                {
                    ApplicationId = applicationId,
                    SharedWithOfqual = true,
                    SharedWithSkillsEngland = true
                }
            };


            _fileServiceMock.Setup(fs => fs.ListBlobs(applicationId.ToString())).Returns(files);
            _fileServiceMock.Setup(fs => fs.OpenReadStreamAsync(It.IsAny<string>()))
                .ReturnsAsync((Stream)null);

            _mediatorMock.Setup(m => m.Send(It.Is<GetApplicationReviewSharingStatusByIdQuery>(query => query.ApplicationReviewId == applicationReviewId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sharingResponse);

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(() => _controller.DownloadAllApplicationFiles(applicationReviewId));
        }

        [Fact]
        public async Task UpdateReviewer_NoDuplicate_RedirectsToViewApplicationAndSetsTempData()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();

            _userHelperServiceMock.Setup(x => x.GetUserType()).Returns(UserType.Qfau);
            _userHelperServiceMock.Setup(x => x.GetUserEmail()).Returns("user@test.com");
            _userHelperServiceMock.Setup(x => x.GetUserDisplayName()).Returns("Test User");

            var reviewerResponse = new BaseMediatrResponse<SaveReviewerCommandResponse>
            {
                Success = true,
                Value = new SaveReviewerCommandResponse
                {
                    DuplicateReviewerError = false
                }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(reviewerResponse);

            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var model = new UpdateReviewerViewModel
            {
                ApplicationReviewId = applicationReviewId,
                ApplicationId = applicationId,
                ReviewerFieldName = nameof(ApplicationReviewViewModel.Reviewer1),
                ReviewerValue = "New Reviewer"
            };

            var result = await _controller.UpdateReviewer(model);

            Assert.Multiple(() =>
            {
                var redirect = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), redirect.ActionName);
                Assert.Equal(applicationReviewId, redirect.RouteValues!["applicationReviewId"]);

                Assert.True(_controller.TempData.ContainsKey("ReviewerUpdated"));
                Assert.Equal(true, _controller.TempData["ReviewerUpdated"]);

                _mediatorMock.Verify(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()), Times.Once);
                _mediatorMock.Verify(m => m.Send(It.IsAny<GetApplicationForReviewByIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
                _mediatorMock.Verify(m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()), Times.Never);

            });
        }


        [Fact]
        public async Task UpdateReviewer_DuplicateReviewer_ReturnsViewApplicationWithModelError()
        {
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var reviewerValue = "Same Reviewer";
            var reviewerFieldName = nameof(ApplicationReviewViewModel.Reviewer1);

            _userHelperServiceMock.Setup(x => x.GetUserType()).Returns(UserType.Qfau);
            _userHelperServiceMock.Setup(x => x.GetUserEmail()).Returns("user@test.com");
            _userHelperServiceMock.Setup(x => x.GetUserDisplayName()).Returns("Test User");

            var reviewerResponse = new BaseMediatrResponse<SaveReviewerCommandResponse>
            {
                Success = true,
                Value = new SaveReviewerCommandResponse
                {
                    DuplicateReviewerError = true
                }
            };

            var reviewResponse = new BaseMediatrResponse<GetApplicationForReviewByIdQueryResponse>
            {
                Success = true,
                Value = _fixture.Create<GetApplicationForReviewByIdQueryResponse>()
            };


            var usersResponse = new BaseMediatrResponse<GetUsersQueryResponse>
            {
                Success = true,
                Value = _fixture.Create<GetUsersQueryResponse>()
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(reviewerResponse);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationForReviewByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(reviewResponse);


            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(usersResponse);

            var model = new UpdateReviewerViewModel
            {
                ApplicationReviewId = applicationReviewId,
                ApplicationId = applicationId,
                ReviewerFieldName = reviewerFieldName,
                ReviewerValue = reviewerValue
            };

            var result = await _controller.UpdateReviewer(model);

            Assert.Multiple(() =>
            {
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal(nameof(ApplicationsReviewController.ViewApplication), viewResult.ViewName);

                Assert.False(_controller.ModelState.IsValid);
                Assert.True(_controller.ModelState.ContainsKey(reviewerFieldName));

                _mediatorMock.Verify(m => m.Send(It.Is<SaveReviewerCommand>(c =>
                    c.ApplicationId == applicationId &&
                    c.ReviewerFieldName == reviewerFieldName &&
                    c.ReviewerValue == reviewerValue &&
                    c.SentByEmail == "user@test.com" &&
                    c.SentByName == "Test User" &&
                    c.UserType == UserType.Qfau.ToString()
                ), It.IsAny<CancellationToken>()), Times.Once);

                _mediatorMock.Verify(m => m.Send(It.Is<GetApplicationForReviewByIdQuery>(q =>
                    q.ApplicationReviewId == applicationReviewId
                ), It.IsAny<CancellationToken>()), Times.Once);
            });
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
