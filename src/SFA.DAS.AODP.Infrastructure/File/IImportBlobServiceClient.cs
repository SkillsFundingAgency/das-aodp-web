using Azure.Storage.Blobs;

namespace SFA.DAS.AODP.Infrastructure.File;

public interface IImportBlobServiceClient
{
    BlobServiceClient Client { get; }
}