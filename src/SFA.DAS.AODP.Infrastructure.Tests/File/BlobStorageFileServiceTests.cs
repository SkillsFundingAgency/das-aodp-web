using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Infrastructure.UnitTests.File
{
    public class BlobStorageFileServiceTests
    {
        private readonly Fixture _fixture = new();
        private readonly BlobStorageSettings _blobStorageSettings;
        private readonly Mock<BlobServiceClient> _blobServiceClient = new();
        private BlobStorageFileService _sut;

        public BlobStorageFileServiceTests()
        {
            _blobStorageSettings = _fixture.Create<BlobStorageSettings>();
            var importBlobStorageSettings = _fixture.Create<ImportBlobStorageSettings>();
            var clientFactoryMock = new Mock<Microsoft.Extensions.Azure.IAzureClientFactory<BlobServiceClient>>();
            clientFactoryMock
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(_blobServiceClient.Object);
            _sut = new(_blobServiceClient.Object, clientFactoryMock.Object, Options.Create(_blobStorageSettings), Options.Create(importBlobStorageSettings));
        }

        [Fact]
        public async Task UploadFileAsync_UploadsFileWithMetadata()
        {
            // Arrange
            string folderName = _fixture.Create<string>();
            string fileName = "Test.docx";
            Stream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test file content"));
            string? contentType = _fixture.Create<string>();
            string fileNamePrefix = _fixture.Create<string>();

            Mock<BlobContainerClient> blobContainerClient = new();
            Mock<BlobClient> blobClient = new();


            _blobServiceClient.Setup(b => b.GetBlobContainerClient(_blobStorageSettings.FileUploadContainerName)).Returns(blobContainerClient.Object);
            blobContainerClient.Setup(b => b.GetBlobClient(It.IsAny<string>())).Returns(blobClient.Object);

            // Act
            await _sut.UploadFileAsync(folderName, fileName, stream, contentType, fileNamePrefix);

            // Assert
            blobClient.Verify(b => b.UploadAsync(
                stream,
                It.IsAny<BlobHttpHeaders>(),
                It.IsAny<Dictionary<string, string>>(),
                default,
                default,
                default,
                default,
                default
            ), Times.Once());

            blobClient.Verify(b => b.UploadAsync(
             It.IsAny<Stream>(),
             It.Is<BlobHttpHeaders>(h => h.ContentType == contentType),
             It.IsAny<Dictionary<string, string>>(),
             default,
             default,
             default,
             default,
             default
            ), Times.Once());


            blobClient.Verify(b => b.UploadAsync(
             It.IsAny<Stream>(),
               It.IsAny<BlobHttpHeaders>(),
               It.Is<Dictionary<string, string>>(d => d[BlobStorageFileService.FileNameMetadataKey] == fileName),
               default,
               default,
               default,
               default,
               default
           ), Times.Once());

            blobClient.Verify(b => b.UploadAsync(
              It.IsAny<Stream>(),
                It.IsAny<BlobHttpHeaders>(),
                It.Is<Dictionary<string, string>>(d => d[BlobStorageFileService.FileExtensionsMetadataKey] == ".docx"),
                default,
                default,
                default,
                default,
                default
            ), Times.Once());


            blobClient.Verify(b => b.UploadAsync(
             It.IsAny<Stream>(),
               It.IsAny<BlobHttpHeaders>(),
               It.Is<Dictionary<string, string>>(d => d[BlobStorageFileService.FilePrefixMetadataKey] == fileNamePrefix),
               default,
               default,
               default,
               default,
               default
            ), Times.Once());
        }



        [Fact]
        public async Task ListBlobs_ReturnsFiles()
        {
            // Arrange
            string folderName = _fixture.Create<string>();
            string fileName = "Test.docx";
            string extension = ".docx";
            string fileNamePrefix = _fixture.Create<string>();

            var responseMock = new Mock<Response>();

            Mock<BlobContainerClient> blobContainerClient = new();
            Mock<BlobClient> blobClient = new();

            var blobItem = BlobsModelFactory.BlobItem(name: fileName);

            var blobItemProperties = BlobsModelFactory.BlobProperties(metadata: new Dictionary<string, string>()
            {
                { BlobStorageFileService.FileNameMetadataKey, fileName },
                { BlobStorageFileService.FileExtensionsMetadataKey, Path.GetExtension(fileName) },
                { BlobStorageFileService.FilePrefixMetadataKey, fileNamePrefix }
            });


            var page = Page<BlobItem>.FromValues(
            [
                blobItem
            ], continuationToken: null, new Mock<Response>().Object);
            var pages = Pageable<BlobItem>.FromPages([page]);

            _blobServiceClient.Setup(b => b.GetBlobContainerClient(_blobStorageSettings.FileUploadContainerName)).Returns(blobContainerClient.Object);

            blobContainerClient.Setup(b => b.GetBlobs(default, default, folderName, default)).Returns(pages);
            blobContainerClient.Setup(b => b.GetBlobClient(fileName)).Returns(blobClient.Object);

            blobClient.Setup(b => b.GetProperties(null, default)).Returns(Response.FromValue(blobItemProperties, responseMock.Object));

            // Act
            var result = _sut.ListBlobs(folderName);

            // Assert
            Assert.NotEmpty(result);
            Assert.Single(result);

            var resultBlobItem = result.First();

            Assert.Equal(fileName, resultBlobItem.FileName);
            Assert.Equal(fileName, resultBlobItem.FullPath);
            Assert.Equal(extension, resultBlobItem.Extension);
            Assert.Equal(fileNamePrefix, resultBlobItem.FileNamePrefix);
        }



        [Fact]
        public async Task GetBlobDetails_ReturnsBlobDetails()
        {
            // Arrange
            string fileName = "Test.docx";
            string extension = ".docx";
            string fileNamePrefix = _fixture.Create<string>();

            var responseMock = new Mock<Response>();

            Mock<BlobContainerClient> blobContainerClient = new();
            Mock<BlobClient> blobClient = new();

            var blobItem = BlobsModelFactory.BlobItem(name: fileName);

            var blobItemProperties = BlobsModelFactory.BlobProperties(metadata: new Dictionary<string, string>()
            {
                { BlobStorageFileService.FileNameMetadataKey, fileName },
                { BlobStorageFileService.FileExtensionsMetadataKey, Path.GetExtension(fileName) },
                { BlobStorageFileService.FilePrefixMetadataKey, fileNamePrefix }
            });

            _blobServiceClient.Setup(b => b.GetBlobContainerClient(_blobStorageSettings.FileUploadContainerName)).Returns(blobContainerClient.Object);

            blobContainerClient.Setup(b => b.GetBlobClient(fileName)).Returns(blobClient.Object);

            blobClient.Setup(b => b.GetPropertiesAsync(null, default)).ReturnsAsync(Response.FromValue(blobItemProperties, responseMock.Object));

            // Act
            var result = await _sut.GetBlobDetails(fileName);

            // Assert
            Assert.Equal(fileName, result.FileName);
            Assert.Equal(fileName, result.FullPath);
            Assert.Equal(extension, result.Extension);
            Assert.Equal(fileNamePrefix, result.FileNamePrefix);
        }


        [Fact]
        public async Task OpenReadStreamAsync_ReturnsStream()
        {
            // Arrange
            string fileName = "Test.docx";
            Stream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Test file content"));

            Mock<BlobContainerClient> blobContainerClient = new();
            Mock<BlobClient> blobClient = new();

            _blobServiceClient.Setup(b => b.GetBlobContainerClient(_blobStorageSettings.FileUploadContainerName)).Returns(blobContainerClient.Object);
            blobContainerClient.Setup(b => b.GetBlobClient(fileName)).Returns(blobClient.Object);

            blobClient.Setup(b => b.OpenReadAsync(0, default, default, default)).ReturnsAsync(stream);

            // Act
            var result = await _sut.OpenReadStreamAsync(fileName);

            // Assert
            Assert.Equal(stream, result);
        }

        [Fact]
        public async Task DeleteFileAsync_CallsDeleteAsyncOnBlobClient()
        {
            // Arrange
            string fileName = "Test.docx";

            Mock<BlobContainerClient> blobContainerClient = new();
            Mock<BlobClient> blobClient = new();

            _blobServiceClient.Setup(b => b.GetBlobContainerClient(_blobStorageSettings.FileUploadContainerName)).Returns(blobContainerClient.Object);
            blobContainerClient.Setup(b => b.GetBlobClient(fileName)).Returns(blobClient.Object);
            // Act
            await _sut.DeleteFileAsync(fileName);

            // Assert
            blobClient.Verify(b => b.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, null,default), Times.Once);
        }
    }
}
