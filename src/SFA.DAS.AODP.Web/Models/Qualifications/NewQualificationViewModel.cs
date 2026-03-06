namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class NewQualificationViewModel
    {
        public Guid QualificationId { get; set; }
        public string? Title { get; set; }
        public string? Reference { get; set; }
        public string? AwardingOrganisation { get; set; }
        public string? Status { get; set; }
        public string? AgeGroup { get; set; }
    }
}
