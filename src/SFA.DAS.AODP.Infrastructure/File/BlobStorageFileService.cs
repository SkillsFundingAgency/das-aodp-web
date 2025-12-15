using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Infrastructure.File
{
    public class BlobStorageFileService : IFileService
    {
        public const string FileNameMetadataKey = "FileName";
        public const string FileExtensionsMetadataKey = "Extension";
        public const string FilePrefixMetadataKey = "FileNamePrefix";
        private readonly BlobStorageSettings _blobStorageSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _blobContainerClient;

        public BlobStorageFileService(BlobServiceClient blobServiceClient, IOptions<BlobStorageSettings> settings)
        {
            _blobServiceClient = blobServiceClient;
            _blobStorageSettings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task UploadFileAsync(string folderName, string fileName, Stream stream, string? contentType, string fileNamePrefix)
        {
            string filePath = $"{folderName}/{Guid.NewGuid()}";

            var blobClient = GetBlobClient(filePath);

            await blobClient.UploadAsync(stream,
                metadata: new Dictionary<string, string>()
                {
                    { FileNameMetadataKey, fileName },
                    { FileExtensionsMetadataKey, Path.GetExtension(fileName) },
                    { FilePrefixMetadataKey, fileNamePrefix }
                },
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

                properties.Value.Metadata.TryGetValue(FileNameMetadataKey, out var fileName);
                properties.Value.Metadata.TryGetValue(FileExtensionsMetadataKey, out var fileExtension);
                properties.Value.Metadata.TryGetValue(FilePrefixMetadataKey, out var filePrefix);

                result.Add(new()
                {
                    FileName = fileName,
                    FullPath = item.Name,
                    Extension = fileExtension,
                    FileNamePrefix = filePrefix,
                });
            }

            return result;
        }

        public async Task UploadXlsxFileAsync(string folderName, string fileName, Stream stream, string? contentType, string fileNamePrefix)
        {
            // Ensure a safe file name is used
            var safeFileName = Path.GetFileName(fileName ?? string.Empty);
            var fileExtension = Path.GetExtension(safeFileName) ?? string.Empty;

            var trimmedFolder = (folderName ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(trimmedFolder) && trimmedFolder.EndsWith("/"))
                trimmedFolder = trimmedFolder.TrimEnd('/');

            var filePath = string.IsNullOrEmpty(trimmedFolder) ? safeFileName : $"{trimmedFolder}/{safeFileName}";

            var blobClient = GetBlobClient(filePath);

            // If a blob with the same path exists delete it (including snapshots) to ensure the uploaded blob uses the exact filename.
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var headers = new BlobHttpHeaders
            {
                ContentDisposition = $"attachment; filename=\"{safeFileName}\""
            };

            if (!string.IsNullOrEmpty(contentType))
            {
                headers.ContentType = contentType;
            }

            await blobClient.UploadAsync(stream,
                metadata: new Dictionary<string, string>()
                {
                    { FileNameMetadataKey, safeFileName },
                    { FileExtensionsMetadataKey, fileExtension },
                    { FilePrefixMetadataKey, fileNamePrefix ?? string.Empty }
                },
                httpHeaders: headers);
        }

        public async Task<UploadedBlob> GetBlobDetails(string fileName)
        {
            EnsureBlobContainerClient();

            var properties = await _blobContainerClient.GetBlobClient(fileName).GetPropertiesAsync();

            return new()
            {
                FileName = properties.Value.Metadata[FileNameMetadataKey],
                FullPath = fileName,
                Extension = properties.Value.Metadata[FileExtensionsMetadataKey],
                FileNamePrefix = properties.Value.Metadata[FilePrefixMetadataKey],
            };
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
        public string FileNamePrefix { get; set; }
        public string FileNameWithPrefix => string.IsNullOrWhiteSpace(FileNamePrefix) ? FileName : $"{FileNamePrefix} {FileName}";
    }
}
