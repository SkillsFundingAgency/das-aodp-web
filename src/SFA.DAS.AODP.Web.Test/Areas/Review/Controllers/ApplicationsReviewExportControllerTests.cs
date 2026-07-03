using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Application.Review;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Helpers.Export;

namespace SFA.DAS.AODP.Web.Test.Areas.Review.Controllers
{
    public class ApplicationsReviewExportControllerTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<ILogger<ApplicationsReviewController>> _loggerMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<IFileService> _fileServiceMock = new();
        private readonly Mock<IApplicationExportService> _exportServiceMock = new();

        private readonly ApplicationsReviewController _controller;

        public ApplicationsReviewExportControllerTests()
        {
            _controller = new ApplicationsReviewController(
                _loggerMock.Object,
                _mediatorMock.Object,
                Mock.Of<SFA.DAS.AODP.Web.Helpers.User.IUserHelperService>(),
                _fileServiceMock.Object,
                Microsoft.Extensions.Options.Options.Create(
                    new SFA.DAS.AODP.Models.Settings.AodpConfiguration()),
                _exportServiceMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.HttpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
        }

        [Fact]
        public async Task DownloadApplicationFormAndFiles_ReturnsZipFile()
        {
            // Arrange
            var applicationReviewId = Guid.NewGuid();
            var applicationId = Guid.NewGuid();
            var submissionId = 6;

            var exportData = new GetApplicationExportDataQueryResponse
            {
                ApplicationMetadata = new ApplicationExportMetadataResponse
                {
                    OrganisationName = "Test Org",
                    Qan = "123456",
                    SubmissionId = submissionId
                }
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                    {
                        ApplicationId = applicationId
                    }
                });

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<GetApplicationExportDataQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationExportDataQueryResponse>
                {
                    Success = true,
                    Value = exportData
                });

            _fileServiceMock
                .Setup(x => x.ListBlobs(applicationId.ToString()))
                .Returns(new List<UploadedBlob>
                {
                    new UploadedBlob { FileName = "questionfile.txt", FullPath = "12345/questionfile.txt" }
                });

            _fileServiceMock
                .Setup(x => x.ListBlobs($"{ApplicationExportConstants.MessageFolderName}/{applicationId.ToString()}"))
                .Returns(new List<UploadedBlob>
                {
                    new UploadedBlob { FileName = "messagefile.txt", FullPath = "messages/12345/messagefile.txt" }
                });

            _exportServiceMock
                .Setup(x => x.GenerateExportZipAsync(
                    It.IsAny<GetApplicationExportDataQueryResponse>(),
                    It.IsAny<List<UploadedBlob>>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            // Act
            var result = await _controller.DownloadApplicationFormAndFiles(applicationReviewId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);

            Assert.Equal("application/zip", fileResult.ContentType);
            Assert.Equal(
                "Test Org_123456_" + submissionId.ToString().PadLeft(6, '0') + ".zip",
                fileResult.FileDownloadName
            );
        }

        [Fact]
        public async Task DownloadApplicationFormAndFiles_WhenQanIsNull_UsesNoQAN()
        {
            // Arrange
            var applicationReviewId = Guid.NewGuid();

            var exportData = new GetApplicationExportDataQueryResponse
            {
                ApplicationMetadata = new ApplicationExportMetadataResponse
                {
                    OrganisationName = "Org",
                    Qan = null,
                    SubmissionId = 2
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationExportDataQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationExportDataQueryResponse>
                {
                    Success = true,
                    Value = exportData
                });

            _mediatorMock
               .Setup(m => m.Send(
                   It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(),
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
               {
                   Success = true,
                   Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                   {
                       ApplicationId = Guid.NewGuid()
                   }
               });

            _fileServiceMock
                .Setup(f => f.ListBlobs(It.IsAny<string>()))
                .Returns(new List<UploadedBlob>());

            _exportServiceMock
                .Setup(s => s.GenerateExportZipAsync(exportData, It.IsAny<List<UploadedBlob>>()))
                .ReturnsAsync(new byte[] { 1 });

            // Act
            var result = await _controller.DownloadApplicationFormAndFiles(applicationReviewId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);

            Assert.Equal("application/zip", fileResult.ContentType);
            Assert.True(fileResult.FileDownloadName.Contains("NoQAN"));
        }

        [Fact]
        public async Task DownloadApplicationFormAndFiles_CallsMediator()
        {
            // Arrange
            var applicationReviewId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetApplicationExportDataQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseMediatrResponse<GetApplicationExportDataQueryResponse>
                {
                    Success = true,
                    Value = new GetApplicationExportDataQueryResponse
                    {
                        ApplicationMetadata = new ApplicationExportMetadataResponse
                        {
                            OrganisationName = "Org",
                            SubmissionId = 3
                        }
                    }
                });

            _mediatorMock
               .Setup(m => m.Send(
                   It.IsAny<GetApplicationReviewSharingStatusByIdQuery>(),
                   It.IsAny<CancellationToken>()))
               .ReturnsAsync(new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
               {
                   Success = true,
                   Value = new GetApplicationReviewSharingStatusByIdQueryResponse
                   {
                       ApplicationId = Guid.NewGuid()
                   }
               });

            _fileServiceMock
                .Setup(f => f.ListBlobs(It.IsAny<string>()))
                .Returns(new List<UploadedBlob>());

            _exportServiceMock
                .Setup(s => s.GenerateExportZipAsync(It.IsAny<GetApplicationExportDataQueryResponse>(), It.IsAny<List<UploadedBlob>>()))
                .ReturnsAsync(new byte[] { 1 });

            // Act
            await _controller.DownloadApplicationFormAndFiles(applicationReviewId);

            // Assert
            _mediatorMock.Verify(m =>
                m.Send(It.IsAny<GetApplicationExportDataQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}