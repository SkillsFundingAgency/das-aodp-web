using SFA.DAS.AODP.Web.Models.GdsComponents;

namespace SFA.DAS.AODP.Web.Models.OutputFile
{
    public enum PublicationDateMode
    {
        None = 0, Today = 1, Manual = 2
    }
    public class OutputFileViewModel
    {
        public PublicationDateMode DateChoice { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }

        public List<OutputFileLogModel> OutputFileLogs { get; set; } = new List<OutputFileLogModel>();
        public bool ParseDate(out DateTime value)
        {
            value = default;
            if (Day is int d && Month is int m && Year is int y)
            {
                try { value = new DateTime(y, m, d); return true; }
                catch { /* invalid calendar date */ }
            }
            return false;
        }
    }
    public class OutputFileLogModel
    {
        public string UserDisplayName { get; set; }
        public DateTime DownloadDate { get; set; }
        public DateTime PublicationDate { get; set; }
        public string ApprovedFileName { get; set; }
        public string ArchivedFileName { get; set; }
    }
}
