namespace SFA.DAS.AODP.Models.Qualifications;

public record ProcessStatusLookup
{
    public static readonly ProcessStatusLookup DecisionRequired = new(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Decision Required");
    public static readonly ProcessStatusLookup NoActionRequired = new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "No Action Required");
    public static readonly ProcessStatusLookup OnHold = new(Guid.Parse("00000000-0000-0000-0000-000000000003"), "On Hold");
    public static readonly ProcessStatusLookup Approved = new(Guid.Parse("00000000-0000-0000-0000-000000000004"), "Approved");
    public static readonly ProcessStatusLookup Rejected = new(Guid.Parse("00000000-0000-0000-0000-000000000005"), "Rejected");

    private static readonly IReadOnlyDictionary<Guid, ProcessStatusLookup> IdLookup = new List<ProcessStatusLookup>
    {
        DecisionRequired, NoActionRequired, OnHold, Approved, Rejected
    }.ToDictionary(x => x.Id);

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    private ProcessStatusLookup(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public static ProcessStatusLookup FromId(Guid id) => IdLookup.Single(o => o.Key == id).Value;

    public static ProcessStatusLookup FromName(string name) => IdLookup.Single(o => o.Value.Name == name).Value;
}