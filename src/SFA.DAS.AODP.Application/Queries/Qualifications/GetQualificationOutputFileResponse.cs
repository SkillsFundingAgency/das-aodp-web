namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationOutputFileResponse
    {
        public byte[] ZipFileContent { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; } = "application/zip";
    }
}
