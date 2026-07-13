namespace SFA.DAS.AODP.Web.Models.Qualifications;

[ExcludeFromCodeCoverage]
public sealed record KeyField(string Key, string DisplayName, KeyFieldPriority Priority)
{
    public static readonly KeyField OrganisationName = new("OrganisationName", "Organisation Name", KeyFieldPriority.Yellow);
    public static readonly KeyField Title = new("Title", "Title", KeyFieldPriority.Yellow);
    public static readonly KeyField Level = new("Level", "Level", KeyFieldPriority.Red);
    public static readonly KeyField Type = new("Type", "Type", KeyFieldPriority.Yellow);
    public static readonly KeyField TotalCredits = new("TotalCredits", "Total Credits", KeyFieldPriority.Yellow);
    public static readonly KeyField Ssa = new("Ssa", "SSA", KeyFieldPriority.Red);
    public static readonly KeyField GradingType = new("GradingType", "Grading Type", KeyFieldPriority.Yellow);
    public static readonly KeyField OfferedInEngland = new("OfferedInEngland", "Offered In England", KeyFieldPriority.Yellow);
    public static readonly KeyField PreSixteen = new("PreSixteen", "Pre-Sixteen", KeyFieldPriority.Yellow);
    public static readonly KeyField SixteenToEighteen = new("SixteenToEighteen", "Sixteen To Eighteen", KeyFieldPriority.Yellow);
    public static readonly KeyField EighteenPlus = new("EighteenPlus", "Eighteen Plus", KeyFieldPriority.Yellow);
    public static readonly KeyField NineteenPlus = new("NineteenPlus", "Nineteen Plus", KeyFieldPriority.Yellow);
    public static readonly KeyField IntentionToSeekFundingInEngland = new("IntentionToSeekFundingInEngland", "Intention to seek funding in England", KeyFieldPriority.Yellow);
    public static readonly KeyField GLH = new("GLH", "Guided learning hours (GLH)", KeyFieldPriority.Red);
    public static readonly KeyField MinimumGlh = new("MinimumGlh", "Minimum GLH", KeyFieldPriority.Yellow);
    public static readonly KeyField Tqt = new("TQT", "Total qualification time (TQT)", KeyFieldPriority.Yellow);
    public static readonly KeyField OperationalEndDate = new("OperationalEndDate", "Operational End Date", KeyFieldPriority.Yellow);
    public static readonly KeyField OfferedInternationally = new("OfferedInternationally", "Offered Internationally", KeyFieldPriority.Yellow);

    public static IReadOnlyList<KeyField> All { get; } =
    [
        OrganisationName, Title, Level, Type, TotalCredits, Ssa, GradingType, OfferedInEngland,
        PreSixteen, SixteenToEighteen, EighteenPlus, NineteenPlus, IntentionToSeekFundingInEngland,
        GLH, MinimumGlh, Tqt, OperationalEndDate, OfferedInternationally
    ];

    public bool Matches(string key) => string.Equals(Key, key, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => DisplayName;
}