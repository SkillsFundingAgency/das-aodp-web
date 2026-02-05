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
                        RelatedLinkText.ViewApplication,
                        url.Action(
                            LinkRoute.Actions.ViewApplication,
                            LinkRoute.Controllers.Applications,
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
                        RelatedLinkText.ViewMessages,
                        url.Action(
                            LinkRoute.Actions.ApplyMessages,
                            LinkRoute.Controllers.ApplicationMessages,
                            new
                            {
                                area = LinkRoute.Areas.Apply,
                                organisationId = ctx.OrganisationId,
                                applicationId = ctx.ApplicationId,
                                formVersionId = ctx.FormVersionId
                            })!,
                        openInNewTab: false)
                },

                RelatedLinksPage.ReviewApplicationMessages => new List<RelatedLink>
                {
                    new RelatedLink(
                        RelatedLinkText.ViewApplication,
                        url.Action(
                            LinkRoute.Actions.ViewApplicationReview,
                            LinkRoute.Controllers.ApplicationsReview,
                            new { applicationReviewId = ctx.ApplicationReviewId })!,
                        openInNewTab: true)
                },

                RelatedLinksPage.ReviewApplicationDetails => new List<RelatedLink>
                {
                    new RelatedLink(
                        RelatedLinkText.ViewMessages,
                        url.Action(
                            LinkRoute.Actions.ReviewMessages,
                            LinkRoute.Controllers.ApplicationMessages,
                            new { area = LinkRoute.Areas.Review, applicationReviewId = ctx.ApplicationReviewId })!,
                        openInNewTab: false),

                    new RelatedLink(
                        RelatedLinkText.ViewApplication,
                        url.Action(
                            LinkRoute.Actions.ViewApplicationReadOnlyDetails,
                            LinkRoute.Controllers.ApplicationsReview,
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
