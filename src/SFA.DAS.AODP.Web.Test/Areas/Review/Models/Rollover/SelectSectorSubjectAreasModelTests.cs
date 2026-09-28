using SFA.DAS.AODP.Domain.ValueObjects;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.Rollover;

public class SelectSectorSubjectAreasModelTests
{
    [Fact]
    public void Set_ShouldMapSectorSubjectAreasAndRestoreSelectedItems()
    {
        var model = new SelectSectorSubjectAreasModel
        {
            SelectedSectorSubjectAreas = [SectorSubjectArea.Engineering]
        };

        var result = model.Set([SectorSubjectArea.Science, SectorSubjectArea.Engineering]);

        result.ShouldBeSameAs(model);
        result.SectorSubjectAreas.Count.ShouldBe(2);
        result.SectorSubjectAreas[0].LabelText.ShouldBe(SectorSubjectArea.Science.Name);
        result.SectorSubjectAreas[0].Value.ShouldBe(SectorSubjectArea.Science.Code);
        result.SectorSubjectAreas[0].IsChecked.ShouldBeFalse();
        result.SectorSubjectAreas[1].IsChecked.ShouldBeTrue();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MarkAllAsChecked_ShouldSelectEverySectorSubjectArea(bool selectCheckboxes)
    {
        var model = new SelectSectorSubjectAreasModel()
            .Set([SectorSubjectArea.Science, SectorSubjectArea.Engineering]);

        var result = model.MarkAllAsChecked(selectCheckboxes);

        result.ShouldBeSameAs(model);
        result.SelectedSectorSubjectAreas.ShouldBe([SectorSubjectArea.Science, SectorSubjectArea.Engineering]);
        result.SectorSubjectAreas.ShouldAllBe(area => area.IsChecked == selectCheckboxes);
    }
}
