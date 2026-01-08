using Azure.Storage.Blobs;

namespace SFA.DAS.AODP.Infrastructure.File;

public class ImportBlobServiceClient : IImportBlobServiceClient
{
    public ImportBlobServiceClient(BlobServiceClient client) => Client = client;
    public BlobServiceClient Client { get; }
}
