namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public record SelectAwardingOrganisationsViewModel
{
    public List<string> SelectedAwardingOrganisations { get; set; } = [];

    public List<CheckboxItem> AwardingOrganisations { get; set; } = [];

    public AwardingOrganisationSelectionType SelectionType { get; set; }

    public static SelectAwardingOrganisationsViewModel Create(
        IEnumerable<AwardingOrganisation> awardingOrganisations,
        IEnumerable<string> selectedAwardingOrganisations,
        AwardingOrganisationSelectionType selectionType = AwardingOrganisationSelectionType.None)
    {
        var organisations = selectedAwardingOrganisations.ToList();
        var selectedIds = organisations.Distinct().ToList();

        return new SelectAwardingOrganisationsViewModel
        {
            SelectionType = selectionType,
            SelectedAwardingOrganisations = selectedIds,
            AwardingOrganisations = awardingOrganisations.Select(o => new CheckboxItem
            {
                Value = o.RecognitionNumber!,
                LabelText = o.NameOfqual ?? o.NameLegal ?? o.NameGovUk ?? o.Acronym ?? o.Id.ToString(),
                IsChecked = selectedIds.Contains(o.RecognitionNumber!)
            }).ToList()
        };
    }

    public SelectAwardingOrganisationsViewModel MarkAllAsChecked(bool selectAllCheckboxes = false)
    {
        if (selectAllCheckboxes)
        {
            AwardingOrganisations.ForEach(o => o.SetChecked());
        }

        SelectedAwardingOrganisations = AwardingOrganisations.Select(o => o.Value).ToList();

        return this;
    }
}
