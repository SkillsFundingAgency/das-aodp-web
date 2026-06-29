using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.TagHelpers;
using SFA.DAS.AODP.Web.Validators.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public record SelectAwardingOrganisationsViewModel
{
    public List<Guid> SelectedAwardingOrganisationIds { get; set; } = [];

    public List<CheckboxItem> AwardingOrganisations { get; set; } = [];

    public AwardingOrganisationSelectionType SelectionType { get; set; }

    public static SelectAwardingOrganisationsViewModel Create(
        IEnumerable<AwardingOrganisation> awardingOrganisations,
        IEnumerable<Guid> selectedAwardingOrganisationIds,
        AwardingOrganisationSelectionType selectionType = AwardingOrganisationSelectionType.None)
    {
        var selectedIds = selectedAwardingOrganisationIds.Distinct().ToList();

        return new SelectAwardingOrganisationsViewModel
        {
            SelectionType = selectionType,
            SelectedAwardingOrganisationIds = selectedIds,
            AwardingOrganisations = awardingOrganisations.Select(o => new CheckboxItem
            {
                Value = o.Id.ToString(),
                LabelText = o.NameOfqual ?? o.NameLegal ?? o.NameGovUk ?? o.Acronym ?? o.Id.ToString(),
                IsChecked = selectedIds.Contains(o.Id)
            }).ToList()
        };
    }
}
