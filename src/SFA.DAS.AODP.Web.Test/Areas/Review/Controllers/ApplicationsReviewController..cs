using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Helpers.User;
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
        private readonly ApplicationsReviewController _controller;
        private readonly IOptions<AodpConfiguration> _aodpOptions = Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/"
        });

        public ApplicationsReviewControllerTests()
        {
            _fixture.Register(() => DateOnly.FromDateTime(new DateTime(2020, 1, 1)));
            _controller = new(_loggerMock.Object, _mediatorMock.Object, _userHelperServiceMock.Object, _fileServiceMock.Object, _aodpOptions);
        }

        [Fact]
        public async Task IndexMethod_PopulatesAndReturnsViewCorrectly()
        {
            //Arrange
            var expectedUserType = UserType.Ofqual;
            _userHelperServiceMock.Setup(x => x.GetUserType()).Returns(expectedUserType);

            var expectedModel = new ApplicationsReviewListViewModel
            {
                Page = 2,
                ItemsPerPage = 10,
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
                    q.Limit == expectedModel.ItemsPerPage &&
                    q.Offset == expectedModel.ItemsPerPage * (expectedModel.Page - 1) &&
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

            _mediatorMock.Setup(m => m.Send(It.IsAny<SaveReviewerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(reviewerResponse);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetApplicationForReviewByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(reviewResponse);

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


    }
}
