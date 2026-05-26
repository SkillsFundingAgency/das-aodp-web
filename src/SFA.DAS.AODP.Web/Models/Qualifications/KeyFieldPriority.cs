namespace SFA.DAS.AODP.Web.Models.Qualifications;

[ExcludeFromCodeCoverage]
public sealed record KeyFieldPriority(string Name, int Rank)
{
    public static readonly KeyFieldPriority Green = new(nameof(Green), 0);
    public static readonly KeyFieldPriority Yellow = new(nameof(Yellow), 1);
    public static readonly KeyFieldPriority Red = new(nameof(Red), 2);

    public override string ToString() => Name;

    public static IReadOnlyList<KeyFieldPriority> All { get; } = [Green, Yellow, Red];

    public static KeyFieldPriority FromName(string name)
    {
        if (string.Equals(name, Green.Name, StringComparison.OrdinalIgnoreCase))
        {
            return Green;
        }

        if (string.Equals(name, Yellow.Name, StringComparison.OrdinalIgnoreCase))
        {
            return Yellow;
        }

        if (string.Equals(name, Red.Name, StringComparison.OrdinalIgnoreCase))
        {
            return Red;
        }

        throw new ArgumentException($"Unknown KeyFieldPriority '{name}'", nameof(name));
    }
}