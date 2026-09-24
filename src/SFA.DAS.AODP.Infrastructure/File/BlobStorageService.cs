using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Polly;
using SFA.DAS.Aodp.Domain.Files;

namespace SFA.DAS.AODP.Infrastructure.File
{
    /*
     * Low‑level wrapper over Azure Blob Storage that upload and reads raw bytes by container and path.
     * */
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IFileStorageLocationPolicy _fileStorageLocationPolicy;

        public BlobStorageService(
            BlobServiceClient blobServiceClient,
            IFileStorageLocationPolicy fileStorageLocationPolicy)
        {
            _blobServiceClient = blobServiceClient;
            _fileStorageLocationPolicy = fileStorageLocationPolicy;
        }

        public async Task<FileStorageLocation> UploadAsync(
            FileCategory category,
            FileContext? context,
            string fileName,
            string? contentType,
            Stream stream)
        {
            var location = _fileStorageLocationPolicy.Resolve(category, context);

            await UploadToLocationAsync(location, fileName, contentType, stream);

            return location;
        }

        public Task UploadAsync(
            FileStorageLocation location,
            string fileName,
            string? contentType,
            Stream stream)
            => UploadToLocationAsync(location, fileName, contentType, stream);

        private async Task UploadToLocationAsync(
            FileStorageLocation location,
            string fileName,
            string? contentType,
            Stream stream)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(location.Container);

            await containerClient.CreateIfNotExistsAsync();

            var blobClient =
                containerClient.GetBlobClient(location.BlobPath);

            var resolvedContentType = string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType;

            var options = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = resolvedContentType,
                    ContentDisposition = $"attachment; filename=\"{fileName}\""
                }
            };

            await blobClient.UploadAsync(stream, options);
        }


        public async Task<Stream> OpenReadStreamAsync(string? containerName, string? blobPath)
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
            ArgumentException.ThrowIfNullOrWhiteSpace(blobPath);

            var containerClient =
                _blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient =
                containerClient.GetBlobClient(blobPath);

            return await blobClient.OpenReadAsync();
        }
    }
}
