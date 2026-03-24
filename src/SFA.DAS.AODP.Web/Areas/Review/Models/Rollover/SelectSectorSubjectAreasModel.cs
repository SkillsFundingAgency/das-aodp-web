using System.Diagnostics.CodeAnalysis;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;
using SFA.DAS.AODP.Web.TagHelpers;
using SFA.DAS.AODP.Web.Validators.Attributes;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public record SelectSectorSubjectAreasModel
{
    //[MustNotBeEmpty(ErrorMessage = "Select if you want to rollover all SSAs or only a selection")]
    public List<SectorSubjectArea> SelectedSectorSubjectAreas { get; set; } = [];

    public List<CheckboxItem> SectorSubjectAreas => SectorSubjectArea.All.Select(o => new CheckboxItem
    {
        LabelText = o.Name,
        Value = o.Name
    }).ToList();

    public SectorSubjectAreaSelectionType SelectionType { get; set; }
}