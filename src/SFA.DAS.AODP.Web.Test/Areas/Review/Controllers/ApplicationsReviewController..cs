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
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Applications;
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
        private readonly Mock<IUserHelperService> _userHelperMock = new();
        private readonly Mock<IFileService> _fileServiceMock = new();

        private readonly ApplicationsReviewController _controller;

        public ApplicationsReviewControllerTests()
        {
            _userHelperMock.Setup(u => u.GetUserType()).Returns(UserType.Ofqual);
            _userHelperMock.Setup(u => u.GetUserEmail()).Returns("user@test.com");
            _userHelperMock.Setup(u => u.GetUserDisplayName()).Returns("Test User");

            var options = Options.Create(new AodpConfiguration
            {
                FindRegulatedQualificationUrl =
                    "https://find-a-qualification.services.ofqual.gov.uk/qualifications/"
            });

            _controller = new ApplicationsReviewController(
                _loggerMock.Object,
                _mediatorMock.Object,
                _userHelperMock.Object,
                _fileServiceMock.Object,
                options);

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
    }
}