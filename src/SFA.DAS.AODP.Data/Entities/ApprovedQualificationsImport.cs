namespace SFA.DAS.AODP.Data.Entities;

public partial class ApprovedQualificationsImport
{
    public int Id { get; set; }

    public DateTime? DateOfOfqualDataSnapshot { get; set; }

    public string? QualificationName { get; set; }

    public string? AwardingOrganisation { get; set; }

    public double? QualificationNumber { get; set; }

    public string? Level { get; set; }

    public string? QualificationType { get; set; }

    public string? Subcategory { get; set; }

    public string? SectorSubjectArea { get; set; }

    public string? Status { get; set; }

    public bool Age1416FundingAvailable { get; set; }

    public string? Age1416FundingApprovalStartDate { get; set; }

    public string? Age1416FundingApprovalEndDate { get; set; }

    public string? Age1416Notes { get; set; }

    public bool Age1619FundingAvailable { get; set; }

    public DateTime? Age1619FundingApprovalStartDate { get; set; }

    public DateTime? Age1619FundingApprovalEndDate { get; set; }

    public string? Age1619Notes { get; set; }

    public bool LocalFlexibilitiesFundingAvailable { get; set; }

    public DateTime? LocalFlexibilitiesFundingApprovalStartDate { get; set; }

    public DateTime? LocalFlexibilitiesFundingApprovalEndDate { get; set; }

    public string? LocalFlexibilitiesNotes { get; set; }

    public bool LegalEntitlementL2l3FundingAvailable { get; set; }

    public string? LegalEntitlementL2l3FundingApprovalStartDate { get; set; }

    public string? LegalEntitlementL2l3FundingApprovalEndDate { get; set; }

    public string? LegalEntitlementL2l3Notes { get; set; }

    public bool LegalEntitlementEnglishandMathsFundingAvailable { get; set; }

    public string? LegalEntitlementEnglishandMathsFundingApprovalStartDate { get; set; }

    public string? LegalEntitlementEnglishandMathsFundingApprovalEndDate { get; set; }

    public string? LegalEntitlementEnglishandMathsNotes { get; set; }

    public bool DigitalEntitlementFundingAvailable { get; set; }

    public string? DigitalEntitlementFundingApprovalStartDate { get; set; }

    public string? DigitalEntitlementFundingApprovalEndDate { get; set; }

    public string? DigitalEntitlementNotes { get; set; }

    public bool Esflevel34FundingAvailable { get; set; }

    public string? Esflevel34FundingApprovalStartDate { get; set; }

    public string? Esflevel34FundingApprovalEndDate { get; set; }

    public string? Esflevel34Notes { get; set; }

    public bool AdvancedLearnerLoansFundingAvailable { get; set; }

    public string? AdvancedLearnerLoansFundingApprovalStartDate { get; set; }

    public string? AdvancedLearnerLoansFundingApprovalEndDate { get; set; }

    public string? AdvancedLearnerLoansNotes { get; set; }

    public string? AwardingOrganisationUrl { get; set; }

    public bool L3freeCoursesForJobsFundingAvailable { get; set; }

    public string? L3freeCoursesForJobsFundingApprovalStartDate { get; set; }

    public string? L3freeCoursesForJobsFundingApprovalEndDate { get; set; }

    public string? L3freeCoursesForJobsNotes { get; set; }

    public string? QualificationNumberVarchar { get; set; }
}
