using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Constants;

namespace SFA.DAS.AODP.Web.Models.RelatedLinks
{
    public static class RelatedLinksBuilder
    {
        public static IReadOnlyList<RelatedLink> Build(
            IUrlHelper url,
            RelatedLinksPage page,
            UserType userType,
            RelatedLinksContext ctx)
        {
            var contextualLinks = page switch
            {
                RelatedLinksPage.ApplyApplicationMessages => new List<RelatedLink>
                {
                    new RelatedLink(
                        RelatedLinkConstants.Text.ViewApplication,
                        url.RouteUrl(
                            RouteNames.Apply_ViewApplication,
                            new
                            {
                                organisationId = ctx.OrganisationId,
                                applicationId = ctx.ApplicationId,
                                formVersionId = ctx.FormVersionId
                            })!,
                        openInNewTab: true)
                },

                RelatedLinksPage.ApplyApplicationDetails => new List<RelatedLink>
                {
                    new RelatedLink(
                        RelatedLinkConstants.Text.ViewMessages,
                        url.RouteUrl(
                            RouteNames.Apply_ApplicationMessages,
                            new
                            {
                                organisationId = ctx.OrganisationId,
                                applicationId = ctx.ApplicationId,
                                formVersionId = ctx.FormVersionId
                            })!,
                        openInNewTab: false)
                },

                RelatedLinksPage.ReviewApplicationMessages => new List<RelatedLink>
                {
                    new RelatedLink(
                        RelatedLinkConstants.Text.ViewApplication,
                        url.RouteUrl(
                            RouteNames.Review_ViewApplication,
                            new { applicationReviewId = ctx.ApplicationReviewId })!,
                        openInNewTab: true)
                },

                RelatedLinksPage.ReviewApplicationDetails => new List<RelatedLink>
                {
                    new RelatedLink(
                        RelatedLinkConstants.Text.ViewMessages,
                        url.RouteUrl(
                            RouteNames.Review_ApplicationMessages,
                            new { applicationReviewId = ctx.ApplicationReviewId })!,
                        openInNewTab: false),

                    new RelatedLink(
                        RelatedLinkConstants.Text.ViewApplication,
                        url.RouteUrl(
                            RouteNames.Review_ViewApplicationReadOnlyDetails,
                            new { applicationReviewId = ctx.ApplicationReviewId })!,
                        openInNewTab: true)
                },

                _ => new List<RelatedLink>()
            };

            return contextualLinks
                .Concat(RelatedLinksConfiguration.ForUser(userType))
                .ToList();
        }
    }
}
