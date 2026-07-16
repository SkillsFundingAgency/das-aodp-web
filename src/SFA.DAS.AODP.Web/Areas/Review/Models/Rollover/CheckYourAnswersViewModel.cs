namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

[ExcludeFromCodeCoverage]
public record CheckYourAnswersViewModel
{
    public IReadOnlyCollection<QualificationLevel> Levels { get; set; } = [];

    public IReadOnlyCollection<QualificationType> Types { get; set; } = [];

    public IReadOnlyCollection<SectorSubjectArea> SectorSubjectAreas
    {
        get => field.OrderBy(o => o.Name).ToList();
        set;
    } = [];

    public IReadOnlyCollection<AwardingOrganisation> AwardingOrganisations
    {
        get => field.OrderBy(o => o.NameOfqual).ToList();
        set;
    } = [];

    public int CandidateCount { get; set; }

    public int AllAwardingOrganisationsCount { get; set; }

    public int AllSectorSubjectAreasCount => SectorSubjectAreas.Count + ExcludedSectorSubjectAreas.Count();

    public IEnumerable<SectorSubjectArea> ExcludedSectorSubjectAreas { get; set; } = [];

    public IEnumerable<AwardingOrganisation> ExcludedAwardingOrganisations
    {
        get => field.OrderBy(o => o.NameOfqual).ToList();
        set;
    } = [];

    public SectorSubjectAreaSelectionType SectorSubjectAreaSelectionType { get; set; }
    
    public AwardingOrganisationSelectionType AwardingOrganisationSelectionType { get; set; }

    public bool ShowAllSectorSubjectAreasSelected => HasLessThan50PercentBeenSelected(SectorSubjectAreas.Count, AllSectorSubjectAreasCount);

    public bool ShowAllAwardingOrganisationsSelected => HasLessThan50PercentBeenSelected(AwardingOrganisations.Count, AllAwardingOrganisationsCount);

    private static bool HasLessThan50PercentBeenSelected(int selected, int total)
    {
        var operandA = (double)selected;
        var operandB = (double)total;

        if (selected > total)
        {
            throw new InvalidOperationException("The total count cannot be less than the selected count.");
        }

        return operandA / operandB <= 0.5;
    }
}
