namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class JobStatusViewModel
    {
        public JobStatusViewModel()
        {
            Name = string.Empty;
            Status = string.Empty;
            LastRunTime = null;
        }

        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? LastRunTime { get; set; }
    }
}