using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;
using SFA.DAS.AODP.Web.TagHelpers;
using SFA.DAS.AODP.Web.Validators.Attributes;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public record SelectQualificationLevelsViewModel
{
    [BindProperty]
    [MustNotBeEmpty(ErrorMessage = "Select the qualification levels you want to rollover")]
    public List<QualificationLevel> SelectedLevels { get; set; } = [];

    public List<CheckboxItem> Levels => QualificationLevel.All.Select(o => new CheckboxItem
    {
        LabelText = o.Name,
        Value = o.Name
    }).ToList();
}

public enum SectorSubjectAreaSelectionType
{
    None = 0,
    All = 1,
    SpecificSelection = 2,
    Unknown = 99
}

public enum AwardingOrganisationSelectionType
{
    None = 0,
    All = 1,
    SpecificSelection = 2,
    Unknown = 99
}