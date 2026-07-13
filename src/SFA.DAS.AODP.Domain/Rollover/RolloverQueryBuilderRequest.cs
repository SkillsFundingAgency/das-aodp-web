namespace SFA.DAS.AODP.Domain.Rollover;

public interface IQueryBuilderFilterRequest;

public sealed record RolloverQueryBuilderRequest(
    IReadOnlyCollection<int> LevelIds,
    IReadOnlyCollection<int> TypeIds,
    IReadOnlyCollection<string> SectorSubjectAreaIds,
    IReadOnlyCollection<string> AwardingOrganisationIds) : IQueryBuilderFilterRequest;

public sealed record RolloverQueryBuilderTypesRequest(
    IReadOnlyCollection<int> LevelIds) : IQueryBuilderFilterRequest;

public sealed record RolloverQueryBuilderSectorSubjectAreaRequest(
    IReadOnlyCollection<int> LevelIds,
    IReadOnlyCollection<int> TypeIds) : IQueryBuilderFilterRequest;

public sealed record RolloverQueryBuilderAwardingOrganisationsRequest(
    IReadOnlyCollection<int> LevelIds,
    IReadOnlyCollection<int> TypeIds,
    IReadOnlyCollection<string> SectorSubjectAreaIds) : IQueryBuilderFilterRequest;

public sealed class RolloverQueryBuilderRequestBuilder
{
    public static RolloverQueryBuilderTypesRequest ForTypes(IEnumerable<int> levelsIds)
    {
        return new RolloverQueryBuilderTypesRequest(levelsIds.ToList());
    }

    public static RolloverQueryBuilderSectorSubjectAreaRequest ForSectorSubjectArea(IEnumerable<int> levelsIds, IEnumerable<int> typeIds)
    {
        return new RolloverQueryBuilderSectorSubjectAreaRequest(levelsIds.ToList(), typeIds.ToList());
    }

    public static RolloverQueryBuilderAwardingOrganisationsRequest ForAwardingOrganisations(IEnumerable<int> levelsIds, IEnumerable<int> typeIds, IEnumerable<string> sectorSubjectAreaIds)
    {
        return new RolloverQueryBuilderAwardingOrganisationsRequest(levelsIds.ToList(), typeIds.ToList(), sectorSubjectAreaIds.ToList());
    }

    public static RolloverQueryBuilderRequest All(IEnumerable<int> levelsIds, IEnumerable<int> typeIds, IEnumerable<string> sectorSubjectAreaIds, IEnumerable<string> awardingOrganisationIds)
    {
        return new RolloverQueryBuilderRequest(levelsIds.ToList(), typeIds.ToList(), sectorSubjectAreaIds.ToList(), awardingOrganisationIds.ToList());
    }
}