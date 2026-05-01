namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

public record FundingStream
{
    public static readonly FundingStream Age1416 = new("Age 14-16");
    public static readonly FundingStream Age1619 = new("Age 16-19");
    public static readonly FundingStream LocalFlexibilities =  new("Local flexibilities");
    public static readonly FundingStream LegalEntitlementL2L3 = new("Legal entitlement L2 L3");
    public static readonly FundingStream LegalEntitlementEnglishAndMaths = new("Legal entitlement English and Maths");
    public static readonly FundingStream DigitalEntitlement = new("Digital entitlement");
    public static readonly FundingStream LifelongLearningEntitlement = new("Lifelong learning entitlement");
    public static readonly FundingStream AdvancedLearnerLoans = new("Advanced learner loans");
    public static readonly FundingStream FreeCoursesForJobs = new("Free courses for jobs");

    public string Name { get; set; } = null!;

    public FundingStream(string name) => Name = name;

    public static readonly IReadOnlyCollection<FundingStream> All = new List<FundingStream>
    {
        Age1416, 
        Age1619, 
        LocalFlexibilities, 
        LegalEntitlementL2L3, 
        LegalEntitlementEnglishAndMaths, 
        DigitalEntitlement, 
        LifelongLearningEntitlement, 
        AdvancedLearnerLoans, 
        FreeCoursesForJobs
    }.OrderBy(o => o.Name).ToList();

    public static bool TryParse(string? value, out FundingStream? result)
    {
        result = All.SingleOrDefault(x => string.Equals(x.Name, value, StringComparison.OrdinalIgnoreCase));
        return result is not null;
    }

    public override string ToString() => Name;
}