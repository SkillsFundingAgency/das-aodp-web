namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

public record QualificationLevel
{
    public static readonly QualificationLevel EntryLevel = new("Entry level");
    public static readonly QualificationLevel Level1 = new("Level 1");
    public static readonly QualificationLevel Level1Or2 = new("Level 1/2");
    public static readonly QualificationLevel Level2 = new("Level 2");
    public static readonly QualificationLevel Level3 = new("Level 3");
    public static readonly QualificationLevel Level4 = new("Level 4");
    public static readonly QualificationLevel Level5 = new("Level 5");
    public static readonly QualificationLevel Level6 = new("Level 6");
    public static readonly QualificationLevel Level7 = new("Level 7");
    public static readonly QualificationLevel Unspecified = new("Unspecified");

    public string Name { get; set; } = null!;

    public QualificationLevel(string name)
    {
        Name = name;
    }

    public static readonly IReadOnlyCollection<QualificationLevel> All = new List<QualificationLevel>
    {
        EntryLevel,Level1, Level1Or2, Level2, Level3, Level4, Level5, Level6, Level7
    };

    public static bool TryParse(string? value, out QualificationLevel? result)
    {
        result = All.SingleOrDefault(x => string.Equals(x.Name, value, StringComparison.OrdinalIgnoreCase));
        return result is not null;
    }

    public override string ToString() => Name;
}