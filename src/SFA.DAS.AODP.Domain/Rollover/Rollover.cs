using System.Diagnostics.CodeAnalysis;
using SFA.DAS.AODP.Domain.ValueObjects;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public record Rollover
{
    public Guid? WorkflowRunId {  get; set; } 
    public RolloverStart? Start { get; set; }

    public RolloverImportStatus? ImportStatus { get; set; }

    public RolloverPreviousData? PreviousData { get; set; }

    public RolloverSelectCandidates? SelectCandidates { get; set; }

    public List<QualificationCandidate> RolloverCandidates { get; set; } = new();
    
    public List<FundingExtensionCandidate>? RolloverFundingExtensionCandidates { get; set; }
    
    public RolloverFundingStream? RolloverFundingStream { get; set; }

    public RolloverEligibilityDates? RolloverEligibilityDates { get; set; }

    public RolloverFundingApprovalEndDate? RolloverFundingApprovalEndDate { get; set; }

    public QueryBuilderFilters QueryBuilderFilters { get; set; } = new();
}

public record QueryBuilderFilters
{
    public IReadOnlyList<QualificationLevel> Levels { get; init; } = [];

    public IReadOnlyList<QualificationType> Types { get; init; } = [];

    public IReadOnlyList<SectorSubjectArea> SectorSubjectAreas { get; init; } = [];

    public IEnumerable<SectorSubjectArea> ExcludedSectorSubjectAreas { get; init; } = [];

    public IEnumerable<AwardingOrganisation> ExcludedAwardingOrganisations { get; init; } = [];

    public SectorSubjectAreaSelectionType SectorSubjectAreasSelectionType { get; set; } =
        SectorSubjectAreaSelectionType.None;

    public IReadOnlyList<string> SelectedAwardingOrganisationIds { get; init; } = [];

    public IReadOnlyList<AwardingOrganisation> SelectedAwardingOrganisations { get; init; } = [];

    public IReadOnlyList<AwardingOrganisation> AllAwardingOrganisations { get; init; } = [];

    public AwardingOrganisationSelectionType AwardingOrganisationSelectionType { get; init; }

    public QueryBuilderFilters SetLevels(IEnumerable<QualificationLevel> levels)
        => this with { Levels = levels.ToList() };

    public QueryBuilderFilters SetTypes(IEnumerable<QualificationType> types) =>
        this with
        {
            Types = types.ToList()
        };

    public QueryBuilderFilters SetSectorSubjectAreas(IEnumerable<SectorSubjectArea> sectorSubjectAreas,
        IEnumerable<SectorSubjectArea> original, SectorSubjectAreaSelectionType selectionType)
    {
        var subjectAreas = sectorSubjectAreas.ToList();
        return this with
        {
            SectorSubjectAreas = subjectAreas,
            ExcludedSectorSubjectAreas = original.Except(subjectAreas),
            SectorSubjectAreasSelectionType = selectionType
        };
    }

    public QueryBuilderFilters SetAwardingOrganisations(
        IEnumerable<string> awardingOrganisationIds,
        IEnumerable<AwardingOrganisation> original,
        AwardingOrganisationSelectionType selectionType = AwardingOrganisationSelectionType.None)
    {
        var awardingOrganisations = awardingOrganisationIds.ToList();
        var allAwardingOrganisations = original.ToList();
        return this with
        {
            AllAwardingOrganisations = allAwardingOrganisations,
            SelectedAwardingOrganisationIds = awardingOrganisations.ToList(),
            SelectedAwardingOrganisations = allAwardingOrganisations
                .Where(o => awardingOrganisations.Contains(o.RecognitionNumber!)).ToList(),
            AwardingOrganisationSelectionType = selectionType,
            ExcludedAwardingOrganisations = allAwardingOrganisations.ExceptBy(awardingOrganisations,
                organisation => organisation.RecognitionNumber)
        };
    }

    public bool CanProgress(out string? missing)
    {
        if (!Levels.Any())
        {
            missing = nameof(Levels);
            return false;
        }

        if (!Types.Any())
        {
            missing = nameof(Types);
            return false;
        }

        if (!SectorSubjectAreas.Any())
        {
            missing = nameof(SectorSubjectAreas);
            return false;
        }

        if (!SelectedAwardingOrganisationIds.Any())
        {
            missing = nameof(SelectedAwardingOrganisationIds);
            return false;
        }

        missing = null;
        return true;
    }
}