


namespace SFA.DAS.AODP.Infrastructure.File
{
    public interface IFileService
    {
        Task DeleteFileAsync(string filePath);
        List<UploadedBlob> ListBlobs(string folderName);
        Task<Stream> OpenReadStreamAsync(string filePath);
        Task UploadFileAsync(string folderName, string fileName, Stream stream, string? contentType);
    }
}