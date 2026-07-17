namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public record CheckYourAnswersViewModel
{
    private IReadOnlyCollection<SectorSubjectArea> _sectorSubjectAreas = new List<SectorSubjectArea>();
    private IReadOnlyCollection<QualificationLevel> _levels = new List<QualificationLevel>();
    private IReadOnlyCollection<QualificationType> _types = new List<QualificationType>();

    public IReadOnlyCollection<QualificationLevel> Levels
    {
        get => GetQualificationLevels();
        set => _levels = value;
    }

    public IReadOnlyCollection<QualificationType> Types
    {
        get => GetQualificationTypes();
        set => _types = value;
    }

    public IReadOnlyCollection<SectorSubjectArea> SectorSubjectAreas
    {
        get => GetSectorSubjectAreas();
        set => _sectorSubjectAreas = value;
    }

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

    private List<SectorSubjectArea> GetSectorSubjectAreas() => _sectorSubjectAreas.OrderBy(o => o.Name).ToList();

    private List<QualificationLevel> GetQualificationLevels() => _levels.OrderBy(o => o.Name).ToList();

    private List<QualificationType> GetQualificationTypes() => _types.OrderBy(o => o.Name).ToList();
}
