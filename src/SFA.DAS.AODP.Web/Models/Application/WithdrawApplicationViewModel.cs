namespace SFA.DAS.AODP.Web.Models.Application
{
    public class WithdrawApplicationViewModel
    {
        public Guid ApplicationId { get; set; }
        public Guid OrganisationId { get; internal set; }
        public Guid FormVersionId { get; internal set; }
    }
}