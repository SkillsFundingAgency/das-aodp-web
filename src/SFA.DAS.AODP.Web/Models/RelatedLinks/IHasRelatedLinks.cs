namespace SFA.DAS.AODP.Web.Models.RelatedLinks
{
    public interface IHasRelatedLinks
    {
        IReadOnlyList<RelatedLink> RelatedLinks { get; }
    }
}
