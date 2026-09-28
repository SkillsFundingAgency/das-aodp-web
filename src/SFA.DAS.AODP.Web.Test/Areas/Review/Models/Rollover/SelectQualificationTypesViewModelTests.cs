using SFA.DAS.AODP.Domain.ValueObjects;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.Rollover;

public class SelectQualificationTypesViewModelTests
{
    [Fact]
    public void Set_ShouldMapTypesAndRestoreSelectedItems()
    {
        var model = new SelectQualificationTypesViewModel
        {
            SelectedTypes = [QualificationType.FunctionalSkills]
        };

        var result = model.Set([QualificationType.GCEAlevel, QualificationType.FunctionalSkills]);

        result.ShouldBeSameAs(model);
        result.Types.Count.ShouldBe(2);
        result.Types[0].LabelText.ShouldBe(QualificationType.GCEAlevel.Name);
        result.Types[0].Value.ShouldBe(QualificationType.GCEAlevel.Id.ToString());
        result.Types[0].IsChecked.ShouldBeFalse();
        result.Types[1].IsChecked.ShouldBeTrue();
    }

    [Fact]
    public void MarkAllAsChecked_ShouldCheckEveryTypeAndReturnSameModel()
    {
        var model = new SelectQualificationTypesViewModel()
            .Set([QualificationType.GCEAlevel, QualificationType.FunctionalSkills]);

        var result = model.MarkAllAsChecked();

        result.ShouldBeSameAs(model);
        result.Types.ShouldAllBe(type => type.IsChecked);
    }
}
