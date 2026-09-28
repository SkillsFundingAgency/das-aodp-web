namespace SFA.DAS.AODP.Models.Qualifications;

public sealed record KeyField(string Key, string DisplayName, KeyFieldPriority Priority)
{
    public static readonly KeyField OrganisationName = new("OrganisationName", "Organisation name", KeyFieldPriority.Yellow);
    public static readonly KeyField Title = new("Title", "Title", KeyFieldPriority.Yellow);
    public static readonly KeyField Level = new("Level", "Level", KeyFieldPriority.Red);
    public static readonly KeyField Type = new("Type", "Type", KeyFieldPriority.Yellow);
    public static readonly KeyField TotalCredits = new("TotalCredits", "Total credits", KeyFieldPriority.Yellow);
    public static readonly KeyField Ssa = new("Ssa", "SSA", KeyFieldPriority.Red);
    public static readonly KeyField GradingType = new("GradingType", "Grading type", KeyFieldPriority.Yellow);
    public static readonly KeyField OfferedInEngland = new("OfferedInEngland", "Offered in England", KeyFieldPriority.Yellow);
    public static readonly KeyField PreSixteen = new("PreSixteen", "Pre-sixteen", KeyFieldPriority.Yellow);
    public static readonly KeyField SixteenToEighteen = new("SixteenToEighteen", "Sixteen to eighteen", KeyFieldPriority.Yellow);
    public static readonly KeyField EighteenPlus = new("EighteenPlus", "Eighteen plus", KeyFieldPriority.Yellow);
    public static readonly KeyField NineteenPlus = new("NineteenPlus", "Nineteen plus", KeyFieldPriority.Yellow);
    public static readonly KeyField IntentionToSeekFundingInEngland = new("IntentionToSeekFundingInEngland", "Intention to seek funding in England", KeyFieldPriority.Yellow);
    public static readonly KeyField Glh = new("GLH", "Guided learning hours (GLH)", KeyFieldPriority.Red);
    public static readonly KeyField MinimumGlh = new("MinimumGlh", "Minimum GLH", KeyFieldPriority.Yellow);
    public static readonly KeyField Tqt = new("TQT", "Total qualification time (TQT)", KeyFieldPriority.Yellow);
    public static readonly KeyField OperationalEndDate = new("OperationalEndDate", "Operational end date", KeyFieldPriority.Yellow);
    public static readonly KeyField OfferedInternationally = new("OfferedInternationally", "Offered internationally", KeyFieldPriority.Yellow);
    public static readonly KeyField EligibleForFunding = new("EligibleForFunding", "Eligibility status", KeyFieldPriority.Red);

    public static IReadOnlyList<KeyField> All { get; } =
    [
        OrganisationName, Title, Level, Type, TotalCredits, Ssa, GradingType, OfferedInEngland,
        PreSixteen, SixteenToEighteen, EighteenPlus, NineteenPlus, IntentionToSeekFundingInEngland,
        Glh, MinimumGlh, Tqt, OperationalEndDate, OfferedInternationally, EligibleForFunding
    ];

    public bool Matches(string key) => string.Equals(Key, key, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => DisplayName;
}