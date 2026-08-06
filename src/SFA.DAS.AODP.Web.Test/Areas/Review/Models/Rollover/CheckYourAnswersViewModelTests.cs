using SFA.DAS.AODP.Domain.ValueObjects;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.Rollover;

public class CheckYourAnswersViewModelTests
{
    [Fact]
    public void Collections_ShouldBeReturnedInDisplayOrder()
    {
        var organisationA = new AwardingOrganisation { NameOfqual = "Alpha" };
        var organisationZ = new AwardingOrganisation { NameOfqual = "Zulu" };
        var model = new CheckYourAnswersViewModel
        {
            SectorSubjectAreas = [SectorSubjectArea.Science, SectorSubjectArea.Engineering],
            AwardingOrganisations = [organisationZ, organisationA],
            ExcludedAwardingOrganisations = [organisationZ, organisationA]
        };

        model.SectorSubjectAreas.ShouldBe([SectorSubjectArea.Engineering, SectorSubjectArea.Science]);
        model.AwardingOrganisations.ShouldBe([organisationA, organisationZ]);
        model.ExcludedAwardingOrganisations.ShouldBe([organisationA, organisationZ]);
    }

    [Fact]
    public void AllSectorSubjectAreasCount_ShouldIncludeSelectedAndExcludedAreas()
    {
        var model = new CheckYourAnswersViewModel
        {
            SectorSubjectAreas = [SectorSubjectArea.Science],
            ExcludedSectorSubjectAreas = [SectorSubjectArea.Engineering, SectorSubjectArea.MathematicsAndStatistics]
        };

        model.AllSectorSubjectAreasCount.ShouldBe(3);
    }

    [Theory]
    [InlineData(1, 3, true)]
    [InlineData(2, 4, true)]
    [InlineData(3, 4, false)]
    public void SelectionSummaries_ShouldUseFiftyPercentThreshold(int selected, int total, bool expected)
    {
        var areas = SectorSubjectArea.All.Take(selected).ToList();
        var organisations = Enumerable.Range(0, selected)
            .Select(index => new AwardingOrganisation { NameOfqual = $"Organisation {index}" })
            .ToList();
        var model = new CheckYourAnswersViewModel
        {
            SectorSubjectAreas = areas,
            ExcludedSectorSubjectAreas = SectorSubjectArea.All.Skip(selected).Take(total - selected),
            AwardingOrganisations = organisations,
            AllAwardingOrganisationsCount = total
        };

        model.ShowAllSectorSubjectAreasSelected.ShouldBe(expected);
        model.ShowAllAwardingOrganisationsSelected.ShouldBe(expected);
    }

    [Fact]
    public void AwardingOrganisationSummary_WhenSelectedCountExceedsTotal_ShouldThrow()
    {
        var model = new CheckYourAnswersViewModel
        {
            AwardingOrganisations =
            [
                new AwardingOrganisation { NameOfqual = "One" },
                new AwardingOrganisation { NameOfqual = "Two" }
            ],
            AllAwardingOrganisationsCount = 1
        };

        Should.Throw<InvalidOperationException>(() => _ = model.ShowAllAwardingOrganisationsSelected)
            .Message.ShouldBe("The total count cannot be less than the selected count.");
    }
}
