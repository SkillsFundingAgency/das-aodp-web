using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

[ExcludeFromCodeCoverage]
public record Rollover
{
    public RolloverStart? Start { get; set; }

    public RolloverImportStatus? ImportStatus { get; set; }

    public RolloverPreviousData? PreviousData { get; set; }

    public RolloverSelectCandidates? SelectCandidates { get; set; }

    public QueryBuilderFilters QueryBuilderFilters { get; set; } = new();
}

public record QueryBuilderFilters
{
    public IList<QualificationLevel> Levels { get; set; } = [];

    public IList<QualificationType> Types { get; set; } = [];

    public IList<SectorSubjectArea> SectorSubjectAreas { get; set; } = [];

    public IList<AwardingOrganisation> AwardingOrganisations { get; set; } = [];

    public QueryBuilderFilters SetLevels(IList<QualificationLevel> levels)
    {
        Levels = levels;

        return this;
    }

    public QueryBuilderFilters SetTypes(IList<QualificationType> types)
    {
        Types = types;

        return this;
    }

    public QueryBuilderFilters SetSectorSubjectAreas(IList<SectorSubjectArea> sectorSubjectAreas)
    {
        SectorSubjectAreas = sectorSubjectAreas;

        return this;
    }

    public QueryBuilderFilters SetAwardingOrganisations(IList<AwardingOrganisation> awardingOrganisations)
    {
        AwardingOrganisations = awardingOrganisations;

        return this;
    }
}