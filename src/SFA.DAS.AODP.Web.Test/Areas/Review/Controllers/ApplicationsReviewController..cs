using AutoFixture;
using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.Cache;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using SFA.DAS.AODP.Web.Areas.Review.Models.Home;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.FormBuilder.Form;
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

        public ApplicationsReviewControllerTests() => _controller = new(_loggerMock.Object, _mediatorMock.Object, _userHelperServiceMock.Object, _fileServiceMock.Object);

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
            var files = new List<UploadedBlob> { new UploadedBlob { FullPath = "file1.txt", FileName= "file1.txt", FileNamePrefix = "Q1" } };
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
            var files = new List<UploadedBlob> { new UploadedBlob { FullPath = "file1.txt", FileName= "file1.txt", FileNamePrefix = "Q1" } };
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

    }
}
