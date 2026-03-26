using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.TagHelpers;
using SFA.DAS.AODP.Web.Validators.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public record SelectAwardingOrganisationsViewModel
{
    public List<AwardingOrganisation> SelectedAwardingOrganisations { get; set; } = [];

    public List<CheckboxItem> AwardingOrganisations { get; set; } = new List<CheckboxItem>
    {
        new CheckboxItem
        {
            Value = "1st Awards Ltd",
            LabelText = "1st Awards Ltd"
        }
    };

    public AwardingOrganisationSelectionType SelectionType { get; set; }
}