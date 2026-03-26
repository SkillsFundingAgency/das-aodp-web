namespace SFA.DAS.AODP.Web.Models.RelatedLinks
{
    public class RelatedLinksContext
    {
        public Guid OrganisationId { get; init; }
        public Guid ApplicationId { get; init; }
        public Guid FormVersionId { get; init; }
        public Guid ApplicationReviewId { get; init; }
    }
}
