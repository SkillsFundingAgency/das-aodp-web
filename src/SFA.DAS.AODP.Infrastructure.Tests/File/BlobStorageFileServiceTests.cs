using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Infrastructure.UnitTests.TestHelpers;
using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Infrastructure.UnitTests.File
{
    public class BlobStorageFileServiceTests
    {
        private readonly Fixture _fixture = new();
        private readonly BlobStorageSettings _blobStorageSettings;
        private readonly FormBuilderSettings _formBuilderSettings;

        private readonly Mock<BlobServiceClient> _blobServiceClient = new();
        private readonly Mock<BlobContainerClient> _safeContainer = new();
        private readonly Mock<BlobContainerClient> _quarantineContainer = new();

        private BlobStorageFileService _sut;

        public BlobStorageFileServiceTests()
        {
            _blobStorageSettings = new BlobStorageSettings
            {
                QuarantineContainerName = "quarantine",
                SafeContainerName = "safe"
            };

            _formBuilderSettings = new FormBuilderSettings
            {
                MaxUploadFileSize = 10,
                UploadFileTypesAllowed = new List<string> { ".docx" }
            };

            var importFileUploadSettings = _fixture.Create<ImportFileUploadSettings>();

            _blobServiceClient
                .Setup(c => c.GetBlobContainerClient("quarantine"))
                .Returns(_quarantineContainer.Object);

            _blobServiceClient
                .Setup(c => c.GetBlobContainerClient("safe"))
                .Returns(_safeContainer.Object);

            _quarantineContainer
                .Setup(c => c.CreateIfNotExists(
                    It.IsAny<PublicAccessType>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()));

            _safeContainer
                .Setup(c => c.CreateIfNotExists(
                    It.IsAny<PublicAccessType>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()));

            _sut = new BlobStorageFileService(
                _blobServiceClient.Object,
                Options.Create(_blobStorageSettings),
                _formBuilderSettings,
                importFileUploadSettings);
        }

        
        [Fact]
        public async Task UploadFileAsync_UploadsFileToQuarantineWithMetadata()
        {
            var prefix = "messages";
            var fileName = "Test.docx";
            var stream = new MemoryStream("Test".Select(c => (byte)c).ToArray());
            var contentType = "application/msword";
            var fileNamePrefix = "Prefix";

            var blobClient = new Mock<BlobClient>();

            _quarantineContainer
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(blobClient.Object);

            await _sut.UploadFileAsync(prefix, fileName, stream, contentType, fileNamePrefix);


            // Assert
            blobClient.Verify(b =>
                b.UploadAsync(
                    It.IsAny<Stream>(),
                    It.Is<BlobUploadOptions>(o =>
                        o.Metadata[BlobStorageFileService.FileNameMetadataKey] == fileName &&
                        o.Metadata[BlobStorageFileService.FileExtensionsMetadataKey] == ".docx" &&
                        o.Metadata[BlobStorageFileService.FilePrefixMetadataKey] == fileNamePrefix &&
                        o.HttpHeaders!.ContentType == contentType),
                    default),
                Times.Once);
        }



        
[Fact]
        public void ListBlobs_ReturnsSafeBlobs()
        {
            var prefix = "messages";
            var fileName = "Test.docx";
            var extension = ".docx";
            var fileNamePrefix = "Prefix";

            var blobItem = BlobsModelFactory.BlobItem(
                name: $"{prefix}/{fileName}",
                metadata: new Dictionary<string, string>
                {
                    { BlobStorageFileService.FileNameMetadataKey, fileName },
                    { BlobStorageFileService.FileExtensionsMetadataKey, extension },
                    { BlobStorageFileService.FilePrefixMetadataKey, fileNamePrefix }
                });

            var page = Page<BlobItem>.FromValues(
                new[] { blobItem },
                null,
                Mock.Of<Response>());

            var pageable = Pageable<BlobItem>.FromPages(new[] { page });

            
            _safeContainer.Setup(c =>
                c.GetBlobs(
                    BlobTraits.Metadata,
                    BlobStates.None,
                    prefix,
                    default))
                .Returns(pageable);

            var result = _sut.ListBlobs(prefix);

            var blob = Assert.Single(result);
            Assert.Equal(fileName, blob.FileName);
            Assert.Equal($"{prefix}/{fileName}", blob.FullPath);
            Assert.Equal(extension, blob.Extension);
            Assert.Equal(fileNamePrefix, blob.FileNamePrefix);
        }


        
public async Task GetBlobDetails_ReturnsMetadataFromSafe()
        {
            var blobPath = "messages/Test.docx";
            var fileNamePrefix = "Prefix";

            var properties = BlobsModelFactory.BlobProperties(
                metadata: new Dictionary<string, string>
                {
                    { BlobStorageFileService.FileNameMetadataKey, "Test.docx" },
                    { BlobStorageFileService.FileExtensionsMetadataKey, ".docx" },
                    { BlobStorageFileService.FilePrefixMetadataKey, fileNamePrefix }
                });

            var blobClient = new Mock<BlobClient>();
            blobClient
                .Setup(b => b.GetPropertiesAsync(null, default))
                .ReturnsAsync(Response.FromValue(properties, Mock.Of<Response>()));

            _safeContainer
                .Setup(c => c.GetBlobClient(blobPath))
                .Returns(blobClient.Object);

            var result = await _sut.GetBlobDetails(blobPath);

            Assert.Equal("Test.docx", result.FileName);
            Assert.Equal(blobPath, result.FullPath);
            Assert.Equal(".docx", result.Extension);
            Assert.Equal(fileNamePrefix, result.FileNamePrefix);
        }


        
        [Fact]
        public async Task OpenReadStreamAsync_ReadsFromSafe()
        {
            var blobPath = "messages/Test.docx";
            var stream = new MemoryStream();

            var blobClient = new Mock<BlobClient>();
            
            blobClient
                .Setup(b => b.OpenReadAsync(0, default, default, default))
                .ReturnsAsync(stream);


            _safeContainer
                .Setup(c => c.GetBlobClient(blobPath))
                .Returns(blobClient.Object);

            var result = await _sut.OpenReadStreamAsync(blobPath);

            Assert.Equal(stream, result);
        }


       
        [Fact]
        public async Task DeleteFileAsync_DeletesFromSafe()
        {
            var blobPath = "messages/Test.docx";

            var blobClient = new Mock<BlobClient>();

            _safeContainer
                .Setup(c => c.GetBlobClient(blobPath))
                .Returns(blobClient.Object);

            await _sut.DeleteFileAsync(blobPath);

            blobClient.Verify(b =>
                b.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.IncludeSnapshots,
                    null,
                    default),
                Times.Once);
        }

    }
}
