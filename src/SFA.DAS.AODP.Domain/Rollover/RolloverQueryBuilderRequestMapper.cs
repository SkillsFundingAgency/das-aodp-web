namespace SFA.DAS.AODP.Domain.Rollover;

public static class RolloverQueryBuilderRequestMapper
{
    public static RolloverQueryBuilderRequest ForAll(QueryBuilderFilters filters)
        => RolloverQueryBuilderRequestBuilder.All(
            filters.Levels.Select(x => x.Id),
            filters.Types.Select(x => x.Id),
            filters.SectorSubjectAreas.Select(x => x.Code),
            filters.SelectedAwardingOrganisationIds
        );

    public static RolloverQueryBuilderAwardingOrganisationsRequest ForAwardingOrganisationFilter(QueryBuilderFilters filters)
    {
        return RolloverQueryBuilderRequestBuilder.ForAwardingOrganisations(
            filters.Levels.Select(x => x.Id),
            filters.Types.Select(x => x.Id),
            filters.SectorSubjectAreas.Select(x => x.Code)
        );
    }

    public static RolloverQueryBuilderTypesRequest ForTypesFilter(QueryBuilderFilters filters)
    {
        return RolloverQueryBuilderRequestBuilder.ForTypes(filters.Levels.Select(x => x.Id));
    }

    public static RolloverQueryBuilderSectorSubjectAreaRequest ForSectorSubjectAreaFilter(QueryBuilderFilters filters)
    {
        return RolloverQueryBuilderRequestBuilder.ForSectorSubjectArea(
            filters.Levels.Select(x => x.Id), 
            filters.Types.Select(x => x.Id));
    }
}