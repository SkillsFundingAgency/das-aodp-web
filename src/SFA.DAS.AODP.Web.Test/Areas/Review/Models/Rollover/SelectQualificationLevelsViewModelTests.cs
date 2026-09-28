using SFA.DAS.AODP.Domain.ValueObjects;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.Rollover;

public class SelectQualificationLevelsViewModelTests
{
    [Fact]
    public void SetLevels_ShouldMapLevelsAndRestoreSelectedItems()
    {
        var model = new SelectQualificationLevelsViewModel
        {
            SelectedLevels = [QualificationLevel.Level3]
        };

        var result = model.SetLevels([QualificationLevel.Level2, QualificationLevel.Level3]);

        result.ShouldBeSameAs(model);
        result.Levels.Count.ShouldBe(2);
        result.Levels[0].LabelText.ShouldBe(QualificationLevel.Level2.Name);
        result.Levels[0].Value.ShouldBe(QualificationLevel.Level2.Id.ToString());
        result.Levels[0].IsChecked.ShouldBeFalse();
        result.Levels[1].IsChecked.ShouldBeTrue();
    }

    [Fact]
    public void MarkAllAsChecked_ShouldCheckEveryLevelAndReturnSameModel()
    {
        var model = new SelectQualificationLevelsViewModel()
            .SetLevels([QualificationLevel.Level2, QualificationLevel.Level3]);

        var result = model.MarkAllAsChecked();

        result.ShouldBeSameAs(model);
        result.Levels.ShouldAllBe(level => level.IsChecked);
    }
}
