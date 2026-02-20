using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;

namespace SFA.DAS.AODP.Infrastructure.UnitTests.TestHelpers
{
    public static class BlobTestFactory
    {
        public static Mock<BlobContainerClient> CreateContainer(
            Pageable<BlobItem>? blobs = null)
        {
            var container = new Mock<BlobContainerClient>();

            container
                .Setup(b => b.CreateIfNotExists(
                    It.IsAny<PublicAccessType>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<BlobContainerEncryptionScopeOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Mock.Of<Response<BlobContainerInfo>>());

            if (blobs != null)
            {
                container
                    .Setup(b => b.GetBlobs(
                        It.IsAny<BlobTraits>(),
                        It.IsAny<BlobStates>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .Returns(blobs);
            }

            return container;
        }

        public static Mock<BlobClient> CreateBlob()
        {
            return new Mock<BlobClient>();
        }
    }
}

