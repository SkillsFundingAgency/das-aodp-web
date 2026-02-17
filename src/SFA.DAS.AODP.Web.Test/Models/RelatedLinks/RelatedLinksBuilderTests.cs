using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Models.RelatedLinks;
namespace SFA.DAS.AODP.Web.UnitTests.Models.RelatedLinks;
public class RelatedLinksBuilderTests
{
    [Fact]
    public void Build_ApplyApplicationMessages_ReturnsContextualAndUserLinks()
    {
        var url = CreateUrlHelper(out var calls);

        var ctx = new RelatedLinksContext
        {
            OrganisationId = Guid.NewGuid(),
            ApplicationId = Guid.NewGuid(),
            FormVersionId = Guid.NewGuid()
        };

        var links = RelatedLinksBuilder.Build(
            url.Object,
            RelatedLinksPage.ApplyApplicationMessages,
            UserType.Qfau,
            ctx);

        Assert.Contains(links, l =>
            l.Text == RelatedLinkConstants.Text.ViewApplication &&
            l.OpenInNewTab == true);

        Assert.Contains(calls, r => r == RouteNames.Apply_ViewApplication);

        Assert.Contains(links, l =>
            l.Text == RelatedLinksConfiguration.FundedQualifications.Text);
    }

    [Fact]
    public void Build_ApplyApplicationDetails_ViewMessages_SameTab()
    {
        var url = CreateUrlHelper(out var calls);

        var ctx = new RelatedLinksContext
        {
            OrganisationId = Guid.NewGuid(),
            ApplicationId = Guid.NewGuid(),
            FormVersionId = Guid.NewGuid()
        };

        var links = RelatedLinksBuilder.Build(
            url.Object,
            RelatedLinksPage.ApplyApplicationDetails,
            UserType.AwardingOrganisation,
            ctx);

        Assert.Contains(links, l =>
            l.Text == RelatedLinkConstants.Text.ViewMessages &&
            l.OpenInNewTab == false);

        Assert.Contains(calls, r => r == RouteNames.Apply_ApplicationMessages);

        Assert.Equal(4, links.Count); // 1 contextual + 3 AO links
    }

    [Fact]
    public void Build_ReviewApplicationMessages_UsesReviewRoute()
    {
        var url = CreateUrlHelper(out var calls);

        var ctx = new RelatedLinksContext
        {
            ApplicationReviewId = Guid.NewGuid()
        };

        var links = RelatedLinksBuilder.Build(
            url.Object,
            RelatedLinksPage.ReviewApplicationMessages,
            UserType.Ofqual,
            ctx);

        Assert.Contains(links, l =>
            l.Text == RelatedLinkConstants.Text.ViewApplication &&
            l.OpenInNewTab == true);

        Assert.Contains(calls, r => r == RouteNames.Review_ViewApplication);
    }

    [Fact]
    public void Build_ReviewApplicationDetails_ReturnsTwoContextualLinks()
    {
        var url = CreateUrlHelper(out var calls);

        var ctx = new RelatedLinksContext
        {
            ApplicationReviewId = Guid.NewGuid()
        };

        var links = RelatedLinksBuilder.Build(
            url.Object,
            RelatedLinksPage.ReviewApplicationDetails,
            UserType.SkillsEngland,
            ctx);

        Assert.Contains(links, l =>
            l.Text == RelatedLinkConstants.Text.ViewMessages &&
            l.OpenInNewTab == false);

        Assert.Contains(links, l =>
            l.Text == RelatedLinkConstants.Text.ViewApplication &&
            l.OpenInNewTab == true);

        Assert.Contains(calls, r => r == RouteNames.Review_ApplicationMessages);
        Assert.Contains(calls, r => r == RouteNames.Review_ViewApplicationReadOnlyDetails);
    }

    [Fact]
    public void Build_UnknownPage_ReturnsOnlyUserLinks()
    {
        var url = CreateUrlHelper(out var calls);

        var links = RelatedLinksBuilder.Build(
            url.Object,
            (RelatedLinksPage)999,
            UserType.Qfau,
            new RelatedLinksContext());

        Assert.Single(links);
        Assert.Equal(RelatedLinksConfiguration.FundedQualifications.Text, links[0].Text);
        Assert.Empty(calls);
    }


    private static Mock<IUrlHelper> CreateUrlHelper(out List<string> routeCalls)
    {
        var calls = new List<string>();   
        routeCalls = calls;               

        var url = new Mock<IUrlHelper>();

        url.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
           .Returns((UrlRouteContext ctx) =>
           {
               calls.Add(ctx.RouteName!); 
               return $"/route/{ctx.RouteName}";
           });

        return url;
    }

}
