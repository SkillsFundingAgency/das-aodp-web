


namespace SFA.DAS.AODP.Infrastructure.File
{
    public interface IFileService
    {
        Task DeleteFileAsync(string filePath);
        Task<UploadedBlob> GetBlobDetails(string fileName);
        List<UploadedBlob> ListBlobs(string folderName);
        Task<Stream> OpenReadStreamAsync(string filePath);
        Task UploadFileAsync(string folderName, string fileName, Stream stream, string? contentType, string fileNamePrefix);
        Task UploadXlsxFileAsync(string folderName, string fileName, Stream stream, string? contentType, string fileNamePrefix);

    }
}