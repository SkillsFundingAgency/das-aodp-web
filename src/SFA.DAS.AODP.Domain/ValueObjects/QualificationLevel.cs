using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Domain.ValueObjects;

[JsonConverter(typeof(QualificationLevelJsonConverter))]
public record QualificationLevel
{
    // You MUST make sure the IDs match between the layers and similarly the Name also has to match exactly, as we use these to lookup values.
    // It's not the most ideal of approaches, the data structure isn't designed well in regard to lookup data so this is a happy medium for the time being.
    // This could also live in a nuget package which all three layers share but again, time is the constraint. 
    public static readonly QualificationLevel EntryLevel = new(0, "Entry level");
    public static readonly QualificationLevel Level1 = new(1, "Level 1");
    public static readonly QualificationLevel Level1Or2 = new(12, "Level 1/Level 2");
    public static readonly QualificationLevel Level2 = new(2, "Level 2");
    public static readonly QualificationLevel Level3 = new(3, "Level 3");
    public static readonly QualificationLevel Level4 = new(4, "Level 4");
    public static readonly QualificationLevel Level5 = new(5, "Level 5");
    public static readonly QualificationLevel Level6 = new(6, "Level 6");
    public static readonly QualificationLevel Level7 = new(7, "Level 7");
    public static readonly QualificationLevel Unspecified = new(99, "Unspecified");

    public int Id { get; }
    public string Name { get; set; } = null!;

    public QualificationLevel(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static readonly IReadOnlyCollection<QualificationLevel> All = new List<QualificationLevel>
    {
        EntryLevel, Level1, Level1Or2, Level2, Level3, Level4, Level5, Level6, Level7
    }.OrderBy(o => o.Name).ToList();

    public static QualificationLevel FromId(int id) => All.FirstOrDefault(o => o.Id == id) ?? Unspecified;

    public static bool TryParse(string? value, out QualificationLevel? result)
    {
        result = All.SingleOrDefault(x =>
            string.Equals(x.Id.ToString(), value, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.Name, value, StringComparison.OrdinalIgnoreCase));
        return result is not null;
    }

    public override string ToString() => Name;
}
