namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public record SelectSectorSubjectAreasModel
{
    public List<SectorSubjectArea> SelectedSectorSubjectAreas { get; set; } = [];

    public List<CheckboxItem> SectorSubjectAreas { get; private set; } = [];

    public SectorSubjectAreaSelectionType SelectionType { get; set; }

    public SelectSectorSubjectAreasModel Set(IEnumerable<SectorSubjectArea> rolloverSsas)
    {
        SectorSubjectAreas = rolloverSsas.Select(r => new CheckboxItem
        {
            LabelText = r.Name,
            Value = r.Code.ToString(),
            IsChecked = SelectedSectorSubjectAreas.Contains(r)
        }).ToList();

        return this;
    }

    public SelectSectorSubjectAreasModel MarkAllAsChecked(bool selectCheckBoxes = false)
    {
        if (selectCheckBoxes)
        {
            SectorSubjectAreas.ForEach(o => o.SetChecked());
        }

        SelectedSectorSubjectAreas = SectorSubjectAreas.Select(o => SectorSubjectArea.FromFullCode(o.Value)).ToList();

        return this;
    }
}
