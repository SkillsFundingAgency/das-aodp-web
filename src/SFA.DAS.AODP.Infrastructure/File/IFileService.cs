


using SFA.DAS.Aodp.Domain.Files;

namespace SFA.DAS.AODP.Infrastructure.File
{
    public interface IFileService
    {

        public Task<Stream> OpenReadStreamAsync(string containerName, string blobPath);

        public Task<FileStorageLocation> UploadAsync(
            FileCategory category,
            FileContext? context,
            string fileName,
            string? contentType,
            Stream stream);
    }
}