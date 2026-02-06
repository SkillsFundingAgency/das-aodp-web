using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
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
        private readonly ImportBlobStorageSettings _importBlobStorageSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _blobContainerClient;
        private readonly IAzureClientFactory<BlobServiceClient> _clientFactory;

        public BlobStorageFileService(BlobServiceClient blobServiceClient,
                    IAzureClientFactory<BlobServiceClient> clientFactory,
                    IOptions<BlobStorageSettings> settings,
                    IOptions<ImportBlobStorageSettings> importSettings)
        {
            _blobServiceClient = blobServiceClient;
            _blobStorageSettings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _importBlobStorageSettings = importSettings.Value ?? throw new ArgumentNullException(nameof(importSettings));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task UploadFileAsync(string folderName, string fileName, Stream stream, string? contentType, string fileNamePrefix)
        {
            string filePath = $"{folderName}/{Guid.NewGuid()}";

            var blobClient = GetBlobClient(filePath, _blobStorageSettings.FileUploadContainerName);

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
            EnsureBlobContainerClient(_blobStorageSettings.FileUploadContainerName);
            var items = _blobContainerClient.GetBlobs(prefix: folderName);
            List<UploadedBlob> result = new List<UploadedBlob>();
            foreach (var item in items)
            {

                var blob = GetBlobClient(item.Name, _blobStorageSettings.FileUploadContainerName);
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
            var safeFileName = Path.GetFileName(fileName ?? string.Empty);
            var fileExtension = Path.GetExtension(safeFileName) ?? string.Empty;

            var trimmedFolder = (folderName ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(trimmedFolder) && trimmedFolder.EndsWith('/'))
                trimmedFolder = trimmedFolder.TrimEnd('/');

            var filePath = string.IsNullOrEmpty(trimmedFolder) ? safeFileName : $"{trimmedFolder}/{safeFileName}";

            BlobServiceClient serviceClient;
            try
            {
                serviceClient = _clientFactory.CreateClient("import");
            }
            catch (InvalidOperationException)
            {
                throw new Exception("Import BlobServiceClient is not configured.");
            }
            var importContainer = serviceClient.GetBlobContainerClient(_importBlobStorageSettings.ImportFilesContainerName);
            await importContainer.CreateIfNotExistsAsync();

            var blobClient = importContainer.GetBlobClient(filePath);

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
            EnsureBlobContainerClient(_blobStorageSettings.FileUploadContainerName);

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
            var blobClient = GetBlobClient(filePath, _blobStorageSettings.FileUploadContainerName);
            var stream = await blobClient.OpenReadAsync();
            return stream;
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var blobClient = GetBlobClient(filePath, _blobStorageSettings.FileUploadContainerName);
            await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        private BlobClient GetBlobClient(string filePath, string containerName)
        {
            EnsureBlobContainerClient(containerName);
            return _blobContainerClient!.GetBlobClient(filePath);
        }

        private void EnsureBlobContainerClient(string containerName)
        {
            if (_blobContainerClient is null)
            {
                _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

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
