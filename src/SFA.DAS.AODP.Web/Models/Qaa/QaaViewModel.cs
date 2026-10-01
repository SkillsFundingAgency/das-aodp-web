namespace SFA.DAS.AODP.Web.Models.Qaa
{
    public class QaaViewModel
    {
        public DateTime? DataLastImportedDate { get; set; }
        public DateTime? MostRecentDownloadDate { get; set; }
        public int NewQualificationsCount { get; set; }
        public int ExtendedQualificationsCount { get; set; }
        public int DiscontinuedQualificationsCount { get; set; }
        public List<QaaDownloadLogModel> DownloadHistory { get; set; } = new();
    }

    public class QaaDownloadLogModel
    {
        public string? UserDisplayName { get; set; }
        public DateTime DownloadDate { get; set; }
    }
}
