using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Models.Users;

namespace SFA.DAS.AODP.Web.Models.RelatedLinks
{
    public interface IHasRelatedLinks
    {
        IReadOnlyList<RelatedLink> RelatedLinks { get; }
        void SetLinks(IUrlHelper url, UserType userType, RelatedLinksContext ctx);
    }
}
