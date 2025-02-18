using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Infrastructure.File
{
    public class BlobStorageFileService : IFileService
    {
        private const string FileNameMetadataKey = "FileName";
        private const string FileExtensionsMetadataKey = "Extension";
        private readonly BlobStorageSettings _blobStorageSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _blobContainerClient;

        public BlobStorageFileService(BlobServiceClient blobServiceClient, IOptions<BlobStorageSettings> settings)
        {
            _blobServiceClient = blobServiceClient;
            _blobStorageSettings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task UploadFileAsync(string folderName, string fileName, Stream stream, string? contentType)
        {
            string filePath = $"{folderName}/{Guid.NewGuid()}";

            var blobClient = GetBlobClient(filePath);

            await blobClient.UploadAsync(stream,
                metadata: new Dictionary<string, string>() { { FileNameMetadataKey, fileName }, { FileExtensionsMetadataKey, Path.GetExtension(fileName) } },
                httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null);
        }

        public List<UploadedBlob> ListBlobs(string folderName)
        {
            EnsureBlobContainerClient();
            var items = _blobContainerClient.GetBlobs(prefix: folderName);
            List<UploadedBlob> result = new List<UploadedBlob>();
            foreach (var item in items)
            {

                var blob = GetBlobClient(item.Name);
                var properties = blob.GetProperties();

                result.Add(new()
                {
                    FileName = properties.Value.Metadata[FileNameMetadataKey],
                    FullPath = item.Name,
                    Extension = properties.Value.Metadata[FileExtensionsMetadataKey]
                });
            }

            return result;
        }


        public async Task<Stream> OpenReadStreamAsync(string filePath)
        {
            var blobClient = GetBlobClient(filePath);
            var stream = await blobClient.OpenReadAsync();
            return stream;
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var blobClient = GetBlobClient(filePath);
            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        private BlobClient GetBlobClient(string filePath)
        {
            EnsureBlobContainerClient();
            return _blobContainerClient!.GetBlobClient(filePath);
        }

        private void EnsureBlobContainerClient()
        {
            if (_blobContainerClient is null)
            {
                _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobStorageSettings.FileUploadContainerName);

                _blobContainerClient.CreateIfNotExists();
            }
        }
    }

    public class UploadedBlob
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
    }
}
