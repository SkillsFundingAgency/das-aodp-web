using System.Diagnostics.CodeAnalysis;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;
using SFA.DAS.AODP.Web.TagHelpers;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public record SelectSectorSubjectAreasModel
{
    public List<SectorSubjectArea> SelectedSectorSubjectAreas { get; set; } = [];

    public List<CheckboxItem> SectorSubjectAreas => SectorSubjectArea.All.Select(o => new CheckboxItem
    {
        LabelText = o.Name,
        Value = o.Name,
        IsChecked = SelectedSectorSubjectAreas.Contains(o)
    }).ToList();

    public SectorSubjectAreaSelectionType SelectionType { get; set; }
}