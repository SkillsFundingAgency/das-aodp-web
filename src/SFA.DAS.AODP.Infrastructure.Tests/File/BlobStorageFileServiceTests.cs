using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.Aodp.Domain.Files;

namespace SFA.DAS.AODP.Infrastructure.UnitTests.File
{
    public class BlobStorageFileServiceTests
    {
        private readonly Mock<BlobServiceClient> _blobServiceClient;
        private readonly Mock<IFileStorageLocationPolicy> _locationPolicy;
        private readonly Mock<BlobContainerClient> _containerClient;
        private readonly Mock<BlobClient> _blobClient;

        private readonly BlobStorageFileService _sut;

        public BlobStorageFileServiceTests()
        {
            _blobServiceClient = new Mock<BlobServiceClient>();
            _locationPolicy = new Mock<IFileStorageLocationPolicy>();
            _containerClient = new Mock<BlobContainerClient>();
            _blobClient = new Mock<BlobClient>();

            _blobServiceClient
                .Setup(b => b.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_containerClient.Object);

            _containerClient
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClient.Object);

            _sut = new BlobStorageFileService(
                _blobServiceClient.Object,
                _locationPolicy.Object);
        }

        [Fact]
        public async Task UploadAsync_Uses_Resolved_Location_From_Policy()
        {
            // Arrange
            var category = FileCategory.QuestionUpload;

            var context = new FileContext(
                ApplicationId: Guid.NewGuid(),
                QuestionId: Guid.NewGuid(),
                MessageId: null);

            var location = new FileStorageLocation(
                Container: "files",
                BlobPath: "some/blob/path");

            _locationPolicy
                .Setup(p => p.Resolve(category, context))
                .Returns(location);

            using var stream = new MemoryStream();

            // Act
            var result = await _sut.UploadAsync(
                category,
                context,
                fileName: "test.docx",
                contentType: "application/msword",
                stream);

            // Assert — policy used
            _locationPolicy.Verify(
                p => p.Resolve(category, context),
                Times.Once);

            // Assert — resolved container + path used
            _blobServiceClient.Verify(
                b => b.GetBlobContainerClient("files"),
                Times.Once);

            _containerClient.Verify(
                c => c.GetBlobClient("some/blob/path"),
                Times.Once);

            // Assert — upload headers
            _blobClient.Verify(b =>
                b.UploadAsync(
                    stream,
                    It.Is<BlobUploadOptions>(o =>
                        o.HttpHeaders.ContentType == "application/msword" &&
                        o.HttpHeaders.ContentDisposition ==
                        "attachment; filename=\"test.docx\""),
                    default),
                Times.Once);

            Assert.Equal(location, result);
        }

        [Fact]
        public async Task UploadAsync_Uses_Default_ContentType_When_ContentType_Is_Empty()
        {
            // Arrange
            var location = new FileStorageLocation("files", "path");

            _locationPolicy
                .Setup(p => p.Resolve(It.IsAny<FileCategory>(), It.IsAny<FileContext>()))
                .Returns(location);

            using var stream = new MemoryStream();

            // Act
            await _sut.UploadAsync(
                FileCategory.QuestionUpload,
                null,
                "file.bin",
                "",
                stream);

            // Assert
            _blobClient.Verify(b =>
                b.UploadAsync(
                    stream,
                    It.Is<BlobUploadOptions>(o =>
                        o.HttpHeaders.ContentType ==
                        "application/octet-stream"),
                    default),
                Times.Once);
        }

        [Fact]
        public async Task OpenReadStreamAsync_Opens_Stream_From_Correct_Container_And_Path()
        {
            // Arrange
            var expectedStream = new MemoryStream();

            _blobClient
                .Setup(b => b.OpenReadAsync())
                .ReturnsAsync(expectedStream);

            // Act
            var result = await _sut.OpenReadStreamAsync(
                "files",
                "some/blob/path");

            // Assert
            _blobServiceClient.Verify(
                b => b.GetBlobContainerClient("files"),
                Times.Once);

            _containerClient.Verify(
                c => c.GetBlobClient("some/blob/path"),
                Times.Once);

            Assert.Equal(expectedStream, result);
        }

        [Theory]
        [InlineData(null, "path")]
        [InlineData("", "path")]
        [InlineData("container", null)]
        [InlineData("container", "")]
        public async Task OpenReadStreamAsync_Throws_For_Invalid_Arguments(
            string container,
            string path)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.OpenReadStreamAsync(container, path));
        }
    }
}