namespace SFA.DAS.AODP.Domain.Rollover;

public sealed record RolloverQueryBuilderRequest
{
    public IReadOnlyCollection<int> LevelIds { get; init; } = [];
    public IReadOnlyCollection<int> TypeIds { get; init; } = [];
    public IReadOnlyCollection<string> SectorSubjectAreaIds { get; init; } = [];
    public IReadOnlyCollection<string> AwardingOrganisationIds { get; init; } = [];

    public static RolloverQueryBuilderRequestBuilder Builder() => new();
}

public sealed class RolloverQueryBuilderRequestBuilder
{
    private readonly HashSet<int> _levelIds = [];
    private readonly HashSet<int> _typeIds = [];
    private readonly HashSet<string> _sectorSubjectAreaIds = [];
    private readonly HashSet<string> _awardingOrganisationIds = [];

    public RolloverQueryBuilderRequestBuilder WithLevels(IEnumerable<int> levelIds)
    {
        AddValues(_levelIds, levelIds.Where(id => id >= 0));
        return this;
    }

    public RolloverQueryBuilderRequestBuilder WithTypes(IEnumerable<int> typeIds)
    {
        AddValues(_typeIds, typeIds.Where(id => id > 0));
        return this;
    }

    public RolloverQueryBuilderRequestBuilder WithSectorSubjectAreas(IEnumerable<string> sectorSubjectAreaIds)
    {
        AddValues(_sectorSubjectAreaIds, sectorSubjectAreaIds.Where(id => !string.IsNullOrWhiteSpace(id)));
        return this;
    }

    public RolloverQueryBuilderRequestBuilder WithAwardingOrganisations(IEnumerable<string> awardingOrganisationIds)
    {
        AddValues(_awardingOrganisationIds, awardingOrganisationIds.Where(id => id != string.Empty));
        return this;
    }

    public RolloverQueryBuilderRequest Build()
        => new()
        {
            LevelIds = _levelIds.OrderBy(id => id).ToArray(),
            TypeIds = _typeIds.OrderBy(id => id).ToArray(),
            SectorSubjectAreaIds = _sectorSubjectAreaIds.OrderBy(id => id).ToArray(),
            AwardingOrganisationIds = _awardingOrganisationIds.OrderBy(id => id).ToArray()
        };

    private static void AddValues<T>(ISet<T> target, IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            target.Add(value);
        }
    }
}
