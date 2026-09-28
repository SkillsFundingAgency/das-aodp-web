using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.Rollover;

public class SelectAwardingOrganisationsViewModelTests
{
    [Fact]
    public void Create_ShouldMapOrganisationsAndRestoreDistinctSelections()
    {
        var organisations = new[]
        {
            new AwardingOrganisation { RecognitionNumber = "RN1", NameOfqual = "Organisation one" },
            new AwardingOrganisation { RecognitionNumber = "RN2", NameLegal = "Organisation two" }
        };

        var result = SelectAwardingOrganisationsViewModel.Create(
            organisations,
            ["RN2", "RN2"],
            AwardingOrganisationSelectionType.SpecificSelection);

        result.SelectionType.ShouldBe(AwardingOrganisationSelectionType.SpecificSelection);
        result.SelectedAwardingOrganisations.ShouldBe(["RN2"]);
        result.AwardingOrganisations.Count.ShouldBe(2);
        result.AwardingOrganisations[0].LabelText.ShouldBe("Organisation one");
        result.AwardingOrganisations[0].Value.ShouldBe("RN1");
        result.AwardingOrganisations[0].IsChecked.ShouldBeFalse();
        result.AwardingOrganisations[1].LabelText.ShouldBe("Organisation two");
        result.AwardingOrganisations[1].IsChecked.ShouldBeTrue();
    }

    [Fact]
    public void Create_ShouldUseAvailableOrganisationNameFallbacks()
    {
        var id = Guid.NewGuid();
        var organisations = new[]
        {
            new AwardingOrganisation { RecognitionNumber = "RN1", NameGovUk = "Gov UK name" },
            new AwardingOrganisation { RecognitionNumber = "RN2", Acronym = "ACR" },
            new AwardingOrganisation { Id = id, RecognitionNumber = "RN3" }
        };

        var result = SelectAwardingOrganisationsViewModel.Create(organisations, []);

        result.AwardingOrganisations.Select(item => item.LabelText)
            .ShouldBe(["Gov UK name", "ACR", id.ToString()]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MarkAllAsChecked_ShouldSelectEveryOrganisation(bool selectCheckboxes)
    {
        var model = SelectAwardingOrganisationsViewModel.Create(
            [
                new AwardingOrganisation { RecognitionNumber = "RN1", NameOfqual = "One" },
                new AwardingOrganisation { RecognitionNumber = "RN2", NameOfqual = "Two" }
            ],
            []);

        var result = model.MarkAllAsChecked(selectCheckboxes);

        result.ShouldBeSameAs(model);
        result.SelectedAwardingOrganisations.ShouldBe(["RN1", "RN2"]);
        result.AwardingOrganisations.ShouldAllBe(organisation => organisation.IsChecked == selectCheckboxes);
    }
}
