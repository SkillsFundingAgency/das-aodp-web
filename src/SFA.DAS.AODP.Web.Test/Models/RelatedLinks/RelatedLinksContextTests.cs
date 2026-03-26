using SFA.DAS.AODP.Web.Models.RelatedLinks;
namespace SFA.DAS.AODP.Web.UnitTests.Models.RelatedLinks;
public class RelatedLinksContextTests
{
    [Fact]
    public void Properties_CanBeInitialised()
    {
        var orgId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        var ctx = new RelatedLinksContext
        {
            OrganisationId = orgId,
            ApplicationId = appId,
            FormVersionId = formId,
            ApplicationReviewId = reviewId
        };

        Assert.Equal(orgId, ctx.OrganisationId);
        Assert.Equal(appId, ctx.ApplicationId);
        Assert.Equal(formId, ctx.FormVersionId);
        Assert.Equal(reviewId, ctx.ApplicationReviewId);
    }
}
