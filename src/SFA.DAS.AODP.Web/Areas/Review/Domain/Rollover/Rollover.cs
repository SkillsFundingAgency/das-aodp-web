using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

[ExcludeFromCodeCoverage]
public record Rollover
{
    public RolloverStart? Start { get; set; }

    public RolloverImportStatus? ImportStatus { get; set; }

    public RolloverPreviousData? PreviousData { get; set; }

    public RolloverSelectCandidates? SelectCandidates { get; set; }

    public List<QualificationCandidate> RolloverCandidates { get; set; } = new();

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

    public IReadOnlyList<Guid> AwardingOrganisationIds { get; init; } = [];

    public AwardingOrganisationSelectionType AwardingOrganisationSelectionType { get; init; }

    public QueryBuilderFilters SetLevels(IEnumerable<QualificationLevel> levels)
        => this with { Levels = levels.ToList() };

    public QueryBuilderFilters SetTypes(IEnumerable<QualificationType> types)
        => this with { Types = types.ToList() };

    public QueryBuilderFilters SetSectorSubjectAreas(IEnumerable<SectorSubjectArea> sectorSubjectAreas)
        => this with { SectorSubjectAreas = sectorSubjectAreas.ToList() };

    public QueryBuilderFilters SetAwardingOrganisations(
        IEnumerable<Guid> awardingOrganisationIds,
        AwardingOrganisationSelectionType selectionType = AwardingOrganisationSelectionType.None)
        => this with
        {
            AwardingOrganisationIds = awardingOrganisationIds.ToList(),
            AwardingOrganisationSelectionType = selectionType
        };
}
