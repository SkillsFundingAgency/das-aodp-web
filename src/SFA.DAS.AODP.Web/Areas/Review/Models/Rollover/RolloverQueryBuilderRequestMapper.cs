using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;

public static class RolloverQueryBuilderRequestMapper
{
    public static RolloverQueryBuilderRequest Map(QueryBuilderFilters filters)
        => RolloverQueryBuilderRequest.Builder()
            .WithLevels(filters.Levels.Select(x => x.Id))
            .WithTypes(filters.Types.Select(x => x.Id))
            .WithSectorSubjectAreas(filters.SectorSubjectAreas.Select(x => x.Code))
            .WithAwardingOrganisations(filters.AwardingOrganisationIds)
            .Build();
}
