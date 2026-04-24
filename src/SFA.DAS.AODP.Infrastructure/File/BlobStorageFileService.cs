using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Infrastructure.Common.IO;
using SFA.DAS.AODP.Models.Settings;

namespace SFA.DAS.AODP.Infrastructure.File
{
    public class BlobStorageFileService : IFileService
    {
        public const string FileNameMetadataKey = "FileName";
        public const string FileExtensionsMetadataKey = "Extension";
        public const string FilePrefixMetadataKey = "FileNamePrefix";
        private readonly BlobStorageSettings _blobStorageSettings;
        private readonly FormBuilderSettings _fileUploadSettings;
        private readonly ImportFileUploadSettings _importFileUploadSettings;
        
        private readonly BlobContainerClient _quarantineContainer;
        private readonly BlobContainerClient _safeContainer;
        private readonly FileUploadValidator _uploadValidator;


        public BlobStorageFileService(
            BlobServiceClient blobServiceClient,
            IOptions<BlobStorageSettings> settings,
            FormBuilderSettings fileUploadSettings,
            ImportFileUploadSettings importFileUploadSettings)
        {

            _blobStorageSettings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

            _quarantineContainer = blobServiceClient.GetBlobContainerClient(_blobStorageSettings.QuarantineContainerName);
            _safeContainer = blobServiceClient.GetBlobContainerClient(_blobStorageSettings.SafeContainerName);
 
            _quarantineContainer.CreateIfNotExists();
            _safeContainer.CreateIfNotExists();

            _uploadValidator = new FileUploadValidator(
                fileUploadSettings
                    ?? throw new ArgumentNullException(nameof(fileUploadSettings)));

            _fileUploadSettings = fileUploadSettings;
            _importFileUploadSettings = importFileUploadSettings;

        }

        public async Task UploadFileAsync(string prefix, string fileName, Stream content, string? contentType, string fileNamePrefix)
        {
            _uploadValidator.ValidateOrThrow(fileName, content);

            var safeFileName = Path.GetFileName(fileName).Trim();

            string blobPath = $"{prefix}/{Guid.NewGuid()}";

            var blobClient = _quarantineContainer.GetBlobClient(blobPath);

            await blobClient.UploadAsync(
                content,
                new BlobUploadOptions
                {
                    HttpHeaders = string.IsNullOrEmpty(contentType)
                        ? null
                        : new BlobHttpHeaders 
                        { 
                            ContentType = contentType, 
                            ContentDisposition = $"attachment; filename=\"{safeFileName}\"" 
                        },
                    Metadata = new Dictionary<string, string>()
                    {
                        { FileNameMetadataKey, safeFileName },
                        { FileExtensionsMetadataKey, Path.GetExtension(safeFileName) },
                        { FilePrefixMetadataKey, fileNamePrefix ?? string.Empty }

                    }
                });
        }

        public List<UploadedBlob> ListBlobs(string prefix)
        {
            var safePrefix = prefix?.Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(safePrefix))
            {
                throw new ArgumentException("Prefix must be provided", nameof(prefix));
            }

            var result = new List<UploadedBlob>();


            foreach (var item in _safeContainer.GetBlobs(
                BlobTraits.Metadata,
                BlobStates.None,
                safePrefix,
                CancellationToken.None))
            {
                item.Metadata.TryGetValue(FileNameMetadataKey, out var fileName);
                item.Metadata.TryGetValue(FileExtensionsMetadataKey, out var fileExtension);
                item.Metadata.TryGetValue(FilePrefixMetadataKey, out var filePrefix);

                result.Add(new UploadedBlob
                {
                    FullPath = item.Name,
                    FileName = fileName ?? string.Empty,
                    Extension = fileExtension ?? string.Empty,
                    FileNamePrefix = filePrefix ?? string.Empty
                });
            }

            return result;
        }

        public async Task UploadXlsxFileAsync(
            string prefix,
            string fileName,
            Stream stream,
            string? contentType,
            string fileNamePrefix)
        {
            var maxAllowedFileSize =
                fileName.Equals("Pldns.xlsx", StringComparison.OrdinalIgnoreCase)
                    ? _importFileUploadSettings.MaxPldnsUploadSizeInMB
                    : _importFileUploadSettings.MaxDefundingListUploadSizeInMB;

            _uploadValidator.ValidateOrThrow(fileName, stream, maxAllowedFileSize);

            var safeFileName = Path.GetFileName(fileName).Trim();
            var fileExtension = Path.GetExtension(safeFileName);

            var safePrefix = prefix?.Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(safePrefix))
            {
                throw new ArgumentException("Prefix must be provided", nameof(prefix));
            }

            var blobPath = $"{safePrefix}/{safeFileName}";
            var blobClient = _quarantineContainer.GetBlobClient(blobPath);

            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            await blobClient.UploadAsync(
                stream,
                metadata: new Dictionary<string, string>
                {
                    { FileNameMetadataKey, safeFileName },
                    { FileExtensionsMetadataKey, fileExtension },
                    { FilePrefixMetadataKey, fileNamePrefix ?? string.Empty }
                },
                httpHeaders: new BlobHttpHeaders
                {
                    ContentType = contentType,
                    ContentDisposition = $"attachment; filename=\"{safeFileName}\""
                });
        }


        public async Task<UploadedBlob> GetBlobDetails(string blobPath)
        {
            
            if (string.IsNullOrWhiteSpace(blobPath))
            {
                throw new ArgumentException("Blob path must be provided", nameof(blobPath));
            }

            var blobClient = _safeContainer.GetBlobClient(blobPath);
            var properties = await blobClient.GetPropertiesAsync();

            properties.Value.Metadata.TryGetValue(FileNameMetadataKey, out var metadataFileName);
            properties.Value.Metadata.TryGetValue(FileExtensionsMetadataKey, out var metadataFileExtension);
            properties.Value.Metadata.TryGetValue(FilePrefixMetadataKey, out var metadataFilePrefix);

            
            return new UploadedBlob
            {
                FileName = metadataFileName ?? string.Empty,
                FullPath = blobPath,
                Extension = metadataFileExtension ?? string.Empty,
                FileNamePrefix = metadataFilePrefix ?? string.Empty
            };
        }

        public async Task<Stream> OpenReadStreamAsync(string blobPath)
        {
            if (string.IsNullOrWhiteSpace(blobPath))
            {
                throw new ArgumentException("Blob path must be provided", nameof(blobPath));
            }

            var blobClient = _safeContainer.GetBlobClient(blobPath);

            return await blobClient.OpenReadAsync();
        }

        public async Task DeleteFileAsync(string blobPath)
        {
            if (string.IsNullOrWhiteSpace(blobPath))
            {
                throw new ArgumentException("Blob path must be provided", nameof(blobPath));
            }

            var blobClient = _safeContainer.GetBlobClient(blobPath);

            await blobClient.DeleteIfExistsAsync(
                DeleteSnapshotsOption.IncludeSnapshots);
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
