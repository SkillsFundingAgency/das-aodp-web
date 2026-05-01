using System.Diagnostics.CodeAnalysis;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;
using SFA.DAS.AODP.Web.TagHelpers;
using SFA.DAS.AODP.Web.Validators.Attributes;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public record SelectQualificationTypesViewModel
{
    [MustNotBeEmpty(ErrorMessage = "Select the qualification types you want to rollover")]
    public List<QualificationType> SelectedTypes { get; set; } = [];

    public List<CheckboxItem> Types => QualificationType.All.Select(o => new CheckboxItem
    {
        LabelText = o.Name,
        Value = o.Name,
        IsChecked = SelectedTypes.Contains(o)
    }).ToList();
}