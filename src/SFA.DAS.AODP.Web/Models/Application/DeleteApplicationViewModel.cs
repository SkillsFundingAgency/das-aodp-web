namespace SFA.DAS.AODP.Web.Models.Application
{
    public class DeleteApplicationViewModel
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int ApplicationReference { get; set; }
        public Guid OrganisationId { get; internal set; }
        public Guid FormVersionId { get; internal set; }
    }
}