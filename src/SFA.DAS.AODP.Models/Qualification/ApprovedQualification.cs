using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration;

namespace SFA.DAS.AODP.Models.Qualification;
public class ApprovedQualification
{
    [Key]
    public int QualificationId { get; set; }
    public string DateOfOfqualDataSnapshot { get; set; }
    public string QualificationName { get; set; }
    public string AwardingOrganisation { get; set; }
    public string QualificationNumber { get; set; }
    public string Level { get; set; }
    public string QualificationType { get; set; }
    public string Subcategory { get; set; }
    public string SectorSubjectArea { get; set; }
    public string Status { get; set; }
    public bool Age1416_FundingAvailable { get; set; }
    public string Age1416_FundingApprovalStartDate { get; set; }
    public string Age1416_FundingApprovalEndDate { get; set; }
    public string Age1416_Notes { get; set; }
    public bool Age1619_FundingAvailable { get; set; }
    public string Age1619_FundingApprovalStartDate { get; set; }
    public string Age1619_FundingApprovalEndDate { get; set; }
    public string Age1619_Notes { get; set; }
    public bool LocalFlexibilities_FundingAvailable { get; set; }
    public string LocalFlexibilities_FundingApprovalStartDate { get; set; }
    public string LocalFlexibilities_FundingApprovalEndDate { get; set; }
    public string LocalFlexibilities_Notes { get; set; }
    public bool LegalEntitlementL2L3_FundingAvailable { get; set; }
    public string LegalEntitlementL2L3_FundingApprovalStartDate { get; set; }
    public string LegalEntitlementL2L3_FundingApprovalEndDate { get; set; }
    public string LegalEntitlementL2L3_Notes { get; set; }
    public bool LegalEntitlementEnglishandMaths_FundingAvailable { get; set; }
    public string LegalEntitlementEnglishandMaths_FundingApprovalStartDate { get; set; }
    public string LegalEntitlementEnglishandMaths_FundingApprovalEndDate { get; set; }
    public string LegalEntitlementEnglishandMaths_Notes { get; set; }
    public bool DigitalEntitlement_FundingAvailable { get; set; }
    public string DigitalEntitlement_FundingApprovalStartDate { get; set; }
    public string DigitalEntitlement_FundingApprovalEndDate { get; set; }
    public string DigitalEntitlement_Notes { get; set; }
    public bool ESFLevel34_FundingAvailable { get; set; }
    public string ESFLevel34_FundingApprovalStartDate { get; set; }
    public string ESFLevel34_FundingApprovalEndDate { get; set; }
    public string ESFLevel34_Notes { get; set; }
    public bool AdvancedLearnerLoans_FundingAvailable { get; set; }
    public string AdvancedLearnerLoans_FundingApprovalStartDate { get; set; }
    public string AdvancedLearnerLoans_FundingApprovalEndDate { get; set; }
    public string AdvancedLearnerLoans_Notes { get; set; }
    public string AwardingOrganisationURL { get; set; }
    public bool L3FreeCoursesForJobs_FundingAvailable { get; set; }
    public string L3FreeCoursesForJobs_FundingApprovalStartDate { get; set; }
    public string L3FreeCoursesForJobs_FundingApprovalEndDate { get; set; }
    public string L3FreeCoursesForJobs_Notes { get; set; }
}

public class ModelClassMap : ClassMap<ApprovedQualification>
{
    public ModelClassMap()
    {
        Map(m => m.DateOfOfqualDataSnapshot).Name("DateOfOfqualDataSnapshot");
        Map(m => m.QualificationName).Name("QualificationName");
        Map(m => m.AwardingOrganisation).Name("AwardingOrganisation");
        Map(m => m.QualificationNumber).Name("QualificationNumber");
        Map(m => m.Level).Name("Level");
        Map(m => m.QualificationType).Name("QualificationType");
        Map(m => m.Subcategory).Name("Subcategory");
        Map(m => m.SectorSubjectArea).Name("SectorSubjectArea");
        Map(m => m.Status).Name("Status");
        Map(m => m.Age1416_FundingAvailable).Name("Age1416_FundingAvailable");
        Map(m => m.Age1416_FundingApprovalStartDate).Name("Age1416_FundingApprovalStartDate");
        Map(m => m.Age1416_FundingApprovalEndDate).Name("Age1416_FundingApprovalEndDate");
        Map(m => m.Age1416_Notes).Name("Age1416_Notes");
        Map(m => m.Age1619_FundingAvailable).Name("Age1619_FundingAvailable");
        Map(m => m.Age1619_FundingApprovalStartDate).Name("Age1619_FundingApprovalStartDate");
        Map(m => m.Age1619_FundingApprovalEndDate).Name("Age1619_FundingApprovalEndDate");
        Map(m => m.Age1619_Notes).Name("Age1619_Notes");
        Map(m => m.LocalFlexibilities_FundingAvailable).Name("LocalFlexibilities_FundingAvailable");
        Map(m => m.LocalFlexibilities_FundingApprovalStartDate).Name("LocalFlexibilities_FundingApprovalStartDate");
        Map(m => m.LocalFlexibilities_FundingApprovalEndDate).Name("LocalFlexibilities_FundingApprovalEndDate");
        Map(m => m.LocalFlexibilities_Notes).Name("LocalFlexibilities_Notes");
        Map(m => m.LegalEntitlementL2L3_FundingAvailable).Name("LegalEntitlementL2L3_FundingAvailable");
        Map(m => m.LegalEntitlementL2L3_FundingApprovalStartDate).Name("LegalEntitlementL2L3_FundingApprovalStartDate");
        Map(m => m.LegalEntitlementL2L3_FundingApprovalEndDate).Name("LegalEntitlementL2L3_FundingApprovalEndDate");
        Map(m => m.LegalEntitlementL2L3_Notes).Name("LegalEntitlementL2L3_Notes");
        Map(m => m.LegalEntitlementEnglishandMaths_FundingAvailable).Name("LegalEntitlementEnglishandMaths_FundingAvailable");
        Map(m => m.LegalEntitlementEnglishandMaths_FundingApprovalStartDate).Name("LegalEntitlementEnglishandMaths_FundingApprovalStartDate");
        Map(m => m.LegalEntitlementEnglishandMaths_FundingApprovalEndDate).Name("LegalEntitlementEnglishandMaths_FundingApprovalEndDate");
        Map(m => m.LegalEntitlementEnglishandMaths_Notes).Name("LegalEntitlementEnglishandMaths_Notes");
        Map(m => m.DigitalEntitlement_FundingAvailable).Name("DigitalEntitlement_FundingAvailable");
        Map(m => m.DigitalEntitlement_FundingApprovalStartDate).Name("DigitalEntitlement_FundingApprovalStartDate");
        Map(m => m.DigitalEntitlement_FundingApprovalEndDate).Name("DigitalEntitlement_FundingApprovalEndDate");
        Map(m => m.DigitalEntitlement_Notes).Name("DigitalEntitlement_Notes");
        Map(m => m.ESFLevel34_FundingAvailable).Name("ESFLevel34_FundingAvailable");
        Map(m => m.ESFLevel34_FundingApprovalStartDate).Name("ESFLevel34_FundingApprovalStartDate");
        Map(m => m.ESFLevel34_FundingApprovalEndDate).Name("ESFLevel34_FundingApprovalEndDate");
        Map(m => m.ESFLevel34_Notes).Name("ESFLevel34_Notes");
        Map(m => m.AdvancedLearnerLoans_FundingAvailable).Name("AdvancedLearnerLoans_FundingAvailable");
        Map(m => m.AdvancedLearnerLoans_FundingApprovalStartDate).Name("AdvancedLearnerLoans_FundingApprovalStartDate");
        Map(m => m.AdvancedLearnerLoans_FundingApprovalEndDate).Name("AdvancedLearnerLoans_FundingApprovalEndDate");
        Map(m => m.AdvancedLearnerLoans_Notes).Name("AdvancedLearnerLoans_Notes");
        Map(m => m.AwardingOrganisationURL).Name("AwardingOrganisationURL");
        Map(m => m.L3FreeCoursesForJobs_FundingAvailable).Name("L3FreeCoursesForJobs_FundingAvailable");
        Map(m => m.L3FreeCoursesForJobs_FundingApprovalStartDate).Name("L3FreeCoursesForJobs_FundingApprovalStartDate");
        Map(m => m.L3FreeCoursesForJobs_FundingApprovalEndDate).Name("L3FreeCoursesForJobs_FundingApprovalEndDate");
        Map(m => m.L3FreeCoursesForJobs_Notes).Name("L3FreeCoursesForJobs_Notes");
    }
}
