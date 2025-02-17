
namespace SFA.DAS.AODP.Infrastructure.File
{
    public interface IFileService
    {
        Task DeleteFileAsync(string filePath);
        Task<Stream> OpenReadStreamAsync(string filePath);
        Task UploadFileAsync(Stream stream, string? contentType);
    }
}