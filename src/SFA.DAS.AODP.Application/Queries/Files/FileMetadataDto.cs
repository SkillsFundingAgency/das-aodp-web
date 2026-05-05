namespace SFA.DAS.AODP.Application.Queries.Files
{
    public class FileMetadataDto
    {
        public Guid FileId { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string BlobContainer { get; init; } = string.Empty;
        public string BlobPath { get; set; } = string.Empty;
        public Guid? ApplicationId { get; init; }
        public Guid? MessageId { get; init; }
        public Guid? QuestionId { get; init; }
        public bool IsDownloadable { get; init; }
    }
}
