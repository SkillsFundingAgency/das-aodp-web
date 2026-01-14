namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationOutputFileResponse
    {
        public byte[] FileContent { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; } = "text/csv";
    }
}
