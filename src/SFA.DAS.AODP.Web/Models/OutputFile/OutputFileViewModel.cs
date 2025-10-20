namespace SFA.DAS.AODP.Web.Models.OutputFile
{
    public class OutputFileViewModel
    {
        public List<OutputFileLogModel> OutputFileLogs { get; set; } = new List<OutputFileLogModel>();
    }
    public class OutputFileLogModel
    {
        public string? UserDisplayName { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? ApprovedFileName { get; set; }
        public string? ArchivedFileName { get; set; }
    }
}
