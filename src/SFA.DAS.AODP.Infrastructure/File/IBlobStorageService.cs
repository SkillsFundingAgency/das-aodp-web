using SFA.DAS.Aodp.Domain.Files;

namespace SFA.DAS.AODP.Infrastructure.File
{
    /// <summary>
    /// Low-level Azure Blob Storage access — raw reads and writes by container/path, with no
    /// knowledge of FileRecord or malware scanning. Only SFA.DAS.AODP.Application.Services.Files.FileService
    /// should depend on this; everything else should go through IFileService instead.
    /// </summary>
    public interface IBlobStorageService
    {
        public Task<Stream> OpenReadStreamAsync(string containerName, string blobPath);

        public Task<FileStorageLocation> UploadAsync(
            FileCategory category,
            FileContext? context,
            string fileName,
            string? contentType,
            Stream stream);

        public Task UploadAsync(
            FileStorageLocation location,
            string fileName,
            string? contentType,
            Stream stream);
    }
}
