using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Infrastructure.Common.IO;
using SFA.DAS.AODP.Models.Common;
using SFA.DAS.AODP.Models.Exceptions;
using SFA.DAS.AODP.Models.Settings;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Infrastructure.File
{
    public class BlobStorageFileService : IFileService
    {
        public const string FileNameMetadataKey = "FileName";
        public const string FileExtensionsMetadataKey = "Extension";
        public const string FilePrefixMetadataKey = "FileNamePrefix";
        private readonly BlobStorageSettings _blobStorageSettings;
        private readonly FormBuilderSettings _fileUploadSettings;
        private readonly ImportBlobStorageSettings _importBlobStorageSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private BlobContainerClient? _blobContainerClient;
        private readonly IAzureClientFactory<BlobServiceClient> _clientFactory;
        private string? _blobContainerName;
        private readonly FileUploadValidator _uploadValidator;

        public const string MalwareScanResultTagKey = "Malware scanning scan result";
        public const string MalwareScanTimeTagKey = "Malware scanning scan time (UTC)";
        public const string MalwareScanCleanValue = "No threats found";
        public const string MalwareScanMaliciousValue = "Malicious";
        public const string MalwareScanErrorValue = "Error";
        public const string MalwareScanNotScannedValue = "Not scanned";

        public BlobStorageFileService(BlobServiceClient blobServiceClient,
                    IAzureClientFactory<BlobServiceClient> clientFactory,
                    IOptions<BlobStorageSettings> settings,
                    IOptions<ImportBlobStorageSettings> importSettings,
                    FormBuilderSettings fileUploadSettings)
        {
            _blobServiceClient = blobServiceClient;
            _blobStorageSettings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _importBlobStorageSettings = importSettings.Value ?? throw new ArgumentNullException(nameof(importSettings));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _fileUploadSettings = fileUploadSettings ?? throw new ArgumentNullException(nameof(fileUploadSettings));
            _uploadValidator = new FileUploadValidator(_fileUploadSettings);
        }

        public async Task UploadFileAsync(string folderName, string fileName, Stream stream, string? contentType, string fileNamePrefix)
        {
            _uploadValidator.ValidateOrThrow(fileName, stream);

            var safeFileName = Path.GetFileName(fileName).Trim();
            var ext = Path.GetExtension(safeFileName);

            string filePath = $"{folderName}/{Guid.NewGuid()}";

            var blobClient = GetBlobClient(filePath, _blobStorageSettings.FileUploadContainerName);

            await blobClient.UploadAsync(stream,
                metadata: new Dictionary<string, string>()
                {
                    { FileNameMetadataKey, safeFileName },
                    { FileExtensionsMetadataKey, ext },
                    { FilePrefixMetadataKey, fileNamePrefix }
                },
                httpHeaders: !string.IsNullOrEmpty(contentType) ? new BlobHttpHeaders { ContentType = contentType } : null);
        }

        public List<UploadedBlob> ListBlobs(string folderName)
        {
            EnsureBlobContainerClient(_blobStorageSettings.FileUploadContainerName);
            var items = _blobContainerClient.GetBlobs(
                BlobTraits.Metadata,
                BlobStates.None,
                prefix: folderName);

            var result = new List<UploadedBlob>();

            foreach (var item in items)
            {
                var metadata = item.Metadata ?? new Dictionary<string, string>();
                metadata.TryGetValue(FileNameMetadataKey, out var fileName);
                metadata.TryGetValue(FileExtensionsMetadataKey, out var fileExtension);
                metadata.TryGetValue(FilePrefixMetadataKey, out var filePrefix);

                var blob = _blobContainerClient.GetBlobClient(item.Name);
                var scanStatus = TryGetScanStatus(blob);

                result.Add(new()
                {
                    FileName = fileName ?? string.Empty,
                    FullPath = item.Name,
                    Extension = fileExtension ?? string.Empty,
                    FileNamePrefix = filePrefix ?? string.Empty,
                    ScanStatus = scanStatus
                });
            }

            return result;
        }

        public async Task UploadXlsxFileAsync(string folderName, string fileName, Stream stream, string? contentType, string fileNamePrefix)
        {
            _uploadValidator.ValidateOrThrow(fileName, stream);

            var safeFileName = Path.GetFileName(fileName).Trim();
            var fileExtension = Path.GetExtension(safeFileName);

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

            var blobClient = _blobContainerClient!.GetBlobClient(fileName);
            var properties = await blobClient.GetPropertiesAsync();

            var scanStatus = TryGetScanStatus(blobClient);

            properties.Value.Metadata.TryGetValue(FileNameMetadataKey, out var metadadaFileName);
            properties.Value.Metadata.TryGetValue(FileExtensionsMetadataKey, out var metadataFileExtension);
            properties.Value.Metadata.TryGetValue(FilePrefixMetadataKey, out var metadataFilePrefix);

            return new()
            {
                FileName = metadadaFileName ?? string.Empty,
                FullPath = fileName,
                Extension = metadataFileExtension ?? string.Empty,
                FileNamePrefix = metadataFilePrefix ?? string.Empty,
                ScanStatus = scanStatus,
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
            if (_blobContainerClient is null || !string.Equals(_blobContainerName, containerName, StringComparison.Ordinal))
            {
                _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                _blobContainerName = containerName;

                _blobContainerClient.CreateIfNotExists();
            }
        }
        private MalwareScanStatus TryGetScanStatus(BlobClient blobClient)
        {
            try
            {
                var tags = blobClient.GetTags();

                if (!tags.Value.Tags.TryGetValue(MalwareScanResultTagKey, out var raw) || string.IsNullOrWhiteSpace(raw))
                    return MalwareScanStatus.InProgress;

                return MapScanStatus(raw);
            }
            catch
            {
                return MalwareScanStatus.Unknown;
            }
        }
        private static MalwareScanStatus MapScanStatus(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return MalwareScanStatus.Unknown;

            return raw switch
            {
                MalwareScanCleanValue => MalwareScanStatus.Clean,
                MalwareScanMaliciousValue => MalwareScanStatus.Malicious,
                MalwareScanErrorValue => MalwareScanStatus.Error,
                MalwareScanNotScannedValue => MalwareScanStatus.InProgress,
                _ => MalwareScanStatus.Unknown
            };
        }
    }

    public class UploadedBlob
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string FileNamePrefix { get; set; }
        public string FileNameWithPrefix => string.IsNullOrWhiteSpace(FileNamePrefix) ? FileName : $"{FileNamePrefix} {FileName}";
        public MalwareScanStatus ScanStatus { get; set; }
    }
}
