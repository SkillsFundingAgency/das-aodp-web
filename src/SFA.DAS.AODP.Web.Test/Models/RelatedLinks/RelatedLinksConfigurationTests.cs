using SFA.DAS.AODP.Web.Models.RelatedLinks;
using SFA.DAS.AODP.Models.Users;
namespace SFA.DAS.AODP.Web.UnitTests.Models.RelatedLinks;
public class RelatedLinksConfigurationTests
{
    [Fact]
    public void ForUser_AwardingOrganisation_ReturnsThreeSpecificLinks()
    {
        var links = RelatedLinksConfiguration.ForUser(UserType.AwardingOrganisation);

        Assert.Equal(3, links.Count);
        Assert.Contains(links, l => l.Text == RelatedLinksConfiguration.FundingApprovalManual.Text);
        Assert.Contains(links, l => l.Text == RelatedLinksConfiguration.OfqualRegulation.Text);
        Assert.Contains(links, l => l.Text == RelatedLinksConfiguration.SkillsEnglandOccupationalMaps.Text);
    }

    [Theory]
    [InlineData(UserType.Qfau)]
    [InlineData(UserType.Ofqual)]
    [InlineData(UserType.SkillsEngland)]
    public void ForUser_Reviewers_ReturnsFundedQualificationsOnly(UserType userType)
    {
        var links = RelatedLinksConfiguration.ForUser(userType);

        Assert.Single(links);
        Assert.Equal(RelatedLinksConfiguration.FundedQualifications.Text, links[0].Text);
    }

    [Fact]
    public void ForUser_UnknownUserType_ReturnsEmpty()
    {
        var links = RelatedLinksConfiguration.ForUser((UserType)999);
        Assert.Empty(links);
    }
}
