using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

namespace SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

public class Rollover
{
    public RolloverStart? Start { get; set; }

    public RolloverImportStatus? ImportStatus { get; set; }

    public QueryBuilderFilters QueryBuilderFilters { get; set; } = new();
}

public class RolloverStart
{
    public RolloverProcess? SelectedProcess { get; set; }
}

public class RolloverImportStatus
{
    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }
    public DateTime? DefundingListLastImported { get; set; }
    public DateTime? PldnsListLastImported { get; set; }
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