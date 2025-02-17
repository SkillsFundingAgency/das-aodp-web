using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Infrastructure.File
{
    public class BlobStorageFileService : IFileService
    {
        private readonly BlobStorageSettings _blobStorageSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _blobContainerClient;

        public BlobStorageFileService(BlobServiceClient blobServiceClient, IOptions<BlobStorageSettings> settings)
        {
            _blobServiceClient = blobServiceClient;
            _blobStorageSettings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task UploadFileAsync(Stream stream, string? contentType)
        {
            string filePath = "";
            var blobClient = await GetBlobClientAsync(filePath);

            await blobClient.UploadAsync(stream, httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null);
        }


        public async Task<Stream> OpenReadStreamAsync(string filePath)
        {
            var blobClient = await GetBlobClientAsync(filePath);
            var stream = await blobClient.OpenReadAsync();
            return stream;
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var blobClient = await GetBlobClientAsync(filePath);
            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        private async Task<BlobClient> GetBlobClientAsync(string filePath)
        {
            await EnsureBlobContainerClientAsync();
            return _blobContainerClient!.GetBlobClient(filePath);
        }

        private async Task EnsureBlobContainerClientAsync()
        {
            if (_blobContainerClient is null)
            {
                _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobStorageSettings.FileUploadContainerName);
                await _blobContainerClient.CreateIfNotExistsAsync();
            }
        }
    }
}
