using Microsoft.EntityFrameworkCore;
using SFA.DAS.AODP.Data.Entities;

namespace SFA.DAS.AODP.Data;

public partial class AodpDbContext : DbContext
{
    public AodpDbContext()
    {
    }

    public AodpDbContext(DbContextOptions<AodpDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ApprovedQualificationsImport> ApprovedQualificationsImports { get; set; }

    public virtual DbSet<ProcessedRegisteredQualification> ProcessedRegisteredQualifications { get; set; }

    public virtual DbSet<RegisteredQualificationsImport> RegisteredQualificationsImports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApprovedQualificationsImport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Approved__3214EC07A1181B65");

            entity.ToTable("ApprovedQualificationsImport");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AdvancedLearnerLoansFundingApprovalEndDate)
                .HasMaxLength(255)
                .HasColumnName("AdvancedLearnerLoans_FundingApprovalEndDate");
            entity.Property(e => e.AdvancedLearnerLoansFundingApprovalStartDate)
                .HasMaxLength(255)
                .HasColumnName("AdvancedLearnerLoans_FundingApprovalStartDate");
            entity.Property(e => e.AdvancedLearnerLoansFundingAvailable).HasColumnName("AdvancedLearnerLoans_FundingAvailable");
            entity.Property(e => e.AdvancedLearnerLoansNotes)
                .HasMaxLength(255)
                .HasColumnName("AdvancedLearnerLoans_Notes");
            entity.Property(e => e.Age1416FundingApprovalEndDate)
                .HasMaxLength(255)
                .HasColumnName("Age1416_FundingApprovalEndDate");
            entity.Property(e => e.Age1416FundingApprovalStartDate)
                .HasMaxLength(255)
                .HasColumnName("Age1416_FundingApprovalStartDate");
            entity.Property(e => e.Age1416FundingAvailable).HasColumnName("Age1416_FundingAvailable");
            entity.Property(e => e.Age1416Notes)
                .HasMaxLength(255)
                .HasColumnName("Age1416_Notes");
            entity.Property(e => e.Age1619FundingApprovalEndDate)
                .HasColumnType("datetime")
                .HasColumnName("Age1619_FundingApprovalEndDate");
            entity.Property(e => e.Age1619FundingApprovalStartDate)
                .HasColumnType("datetime")
                .HasColumnName("Age1619_FundingApprovalStartDate");
            entity.Property(e => e.Age1619FundingAvailable).HasColumnName("Age1619_FundingAvailable");
            entity.Property(e => e.Age1619Notes)
                .HasMaxLength(255)
                .HasColumnName("Age1619_Notes");
            entity.Property(e => e.AwardingOrganisation).HasMaxLength(255);
            entity.Property(e => e.AwardingOrganisationUrl)
                .HasMaxLength(255)
                .HasColumnName("AwardingOrganisationURL");
            entity.Property(e => e.DateOfOfqualDataSnapshot).HasColumnType("datetime");
            entity.Property(e => e.DigitalEntitlementFundingApprovalEndDate)
                .HasMaxLength(255)
                .HasColumnName("DigitalEntitlement_FundingApprovalEndDate");
            entity.Property(e => e.DigitalEntitlementFundingApprovalStartDate)
                .HasMaxLength(255)
                .HasColumnName("DigitalEntitlement_FundingApprovalStartDate");
            entity.Property(e => e.DigitalEntitlementFundingAvailable).HasColumnName("DigitalEntitlement_FundingAvailable");
            entity.Property(e => e.DigitalEntitlementNotes)
                .HasMaxLength(255)
                .HasColumnName("DigitalEntitlement_Notes");
            entity.Property(e => e.Esflevel34FundingApprovalEndDate)
                .HasMaxLength(255)
                .HasColumnName("ESFLevel34_FundingApprovalEndDate");
            entity.Property(e => e.Esflevel34FundingApprovalStartDate)
                .HasMaxLength(255)
                .HasColumnName("ESFLevel34_FundingApprovalStartDate");
            entity.Property(e => e.Esflevel34FundingAvailable).HasColumnName("ESFLevel34_FundingAvailable");
            entity.Property(e => e.Esflevel34Notes)
                .HasMaxLength(255)
                .HasColumnName("ESFLevel34_Notes");
            entity.Property(e => e.L3freeCoursesForJobsFundingApprovalEndDate)
                .HasMaxLength(255)
                .HasColumnName("L3FreeCoursesForJobs_FundingApprovalEndDate");
            entity.Property(e => e.L3freeCoursesForJobsFundingApprovalStartDate)
                .HasMaxLength(255)
                .HasColumnName("L3FreeCoursesForJobs_FundingApprovalStartDate");
            entity.Property(e => e.L3freeCoursesForJobsFundingAvailable).HasColumnName("L3FreeCoursesForJobs_FundingAvailable");
            entity.Property(e => e.L3freeCoursesForJobsNotes)
                .HasMaxLength(255)
                .HasColumnName("L3FreeCoursesForJobs_Notes");
            entity.Property(e => e.LegalEntitlementEnglishandMathsFundingApprovalEndDate)
                .HasMaxLength(255)
                .HasColumnName("LegalEntitlementEnglishandMaths_FundingApprovalEndDate");
            entity.Property(e => e.LegalEntitlementEnglishandMathsFundingApprovalStartDate)
                .HasMaxLength(255)
                .HasColumnName("LegalEntitlementEnglishandMaths_FundingApprovalStartDate");
            entity.Property(e => e.LegalEntitlementEnglishandMathsFundingAvailable).HasColumnName("LegalEntitlementEnglishandMaths_FundingAvailable");
            entity.Property(e => e.LegalEntitlementEnglishandMathsNotes)
                .HasMaxLength(255)
                .HasColumnName("LegalEntitlementEnglishandMaths_Notes");
            entity.Property(e => e.LegalEntitlementL2l3FundingApprovalEndDate)
                .HasMaxLength(255)
                .HasColumnName("LegalEntitlementL2L3_FundingApprovalEndDate");
            entity.Property(e => e.LegalEntitlementL2l3FundingApprovalStartDate)
                .HasMaxLength(255)
                .HasColumnName("LegalEntitlementL2L3_FundingApprovalStartDate");
            entity.Property(e => e.LegalEntitlementL2l3FundingAvailable).HasColumnName("LegalEntitlementL2L3_FundingAvailable");
            entity.Property(e => e.LegalEntitlementL2l3Notes)
                .HasMaxLength(255)
                .HasColumnName("LegalEntitlementL2L3_Notes");
            entity.Property(e => e.Level).HasMaxLength(255);
            entity.Property(e => e.LocalFlexibilitiesFundingApprovalEndDate)
                .HasColumnType("datetime")
                .HasColumnName("LocalFlexibilities_FundingApprovalEndDate");
            entity.Property(e => e.LocalFlexibilitiesFundingApprovalStartDate)
                .HasColumnType("datetime")
                .HasColumnName("LocalFlexibilities_FundingApprovalStartDate");
            entity.Property(e => e.LocalFlexibilitiesFundingAvailable).HasColumnName("LocalFlexibilities_FundingAvailable");
            entity.Property(e => e.LocalFlexibilitiesNotes)
                .HasMaxLength(255)
                .HasColumnName("LocalFlexibilities_Notes");
            entity.Property(e => e.QualificationName).HasMaxLength(255);
            entity.Property(e => e.QualificationNumberVarchar)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.QualificationType).HasMaxLength(255);
            entity.Property(e => e.SectorSubjectArea).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(255);
            entity.Property(e => e.Subcategory).HasMaxLength(255);
        });

        modelBuilder.Entity<ProcessedRegisteredQualification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ProcessedRegisteredQualification");

            entity.Property(e => e.ApprenticeshipStandardReferenceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ApprenticeshipStandardTitle)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ApprovedForDelfundedProgramme)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("ApprovedForDELFundedProgramme");
            entity.Property(e => e.AssessmentMethods).IsUnicode(false);
            entity.Property(e => e.CertificationEndDate).HasColumnType("datetime");
            entity.Property(e => e.EntitlementFrameworkDesignation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EqfLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GceSizeEquivalence)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GcseSizeEquivalence)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GradingScale)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.GradingType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InsertedDate).HasColumnType("datetime");
            entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LinkToSpecification).IsUnicode(false);
            entity.Property(e => e.MaximumGlh).HasColumnName("MaximumGLH");
            entity.Property(e => e.MinimumGlh).HasColumnName("MinimumGLH");
            entity.Property(e => e.NiDiscountCode)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.OperationalEndDate).HasColumnType("datetime");
            entity.Property(e => e.OperationalStartDate).HasColumnType("datetime");
            entity.Property(e => e.OrganisationAcronym)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrganisationName)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.OrganisationRecognitionNumber)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Pathways).IsUnicode(false);
            entity.Property(e => e.QualificationNumber)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.QualificationNumberNoObliques)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RegulationStartDate).HasColumnType("datetime");
            entity.Property(e => e.ReviewDate).HasColumnType("datetime");
            entity.Property(e => e.Specialism).IsUnicode(false);
            entity.Property(e => e.Ssa)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UiLastUpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<RegisteredQualificationsImport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_RegisteredQualification");

            entity.ToTable("RegisteredQualificationsImport");

            entity.Property(e => e.ApprenticeshipStandardReferenceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ApprenticeshipStandardTitle)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ApprovedForDelfundedProgramme)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("ApprovedForDELFundedProgramme");
            entity.Property(e => e.AssessmentMethods).IsUnicode(false);
            entity.Property(e => e.CertificationEndDate).HasColumnType("datetime");
            entity.Property(e => e.ChangedFields).IsUnicode(false);
            entity.Property(e => e.EntitlementFrameworkDesignation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EqfLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GceSizeEquivalence)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GcseSizeEquivalence)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GradingScale)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.GradingType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ImportStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("New");
            entity.Property(e => e.InsertedDate).HasColumnType("datetime");
            entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.Level)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LinkToSpecification).IsUnicode(false);
            entity.Property(e => e.MaximumGlh).HasColumnName("MaximumGLH");
            entity.Property(e => e.MinimumGlh).HasColumnName("MinimumGLH");
            entity.Property(e => e.NiDiscountCode)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.OperationalEndDate).HasColumnType("datetime");
            entity.Property(e => e.OperationalStartDate).HasColumnType("datetime");
            entity.Property(e => e.OrganisationAcronym)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrganisationName)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.OrganisationRecognitionNumber)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Pathways).IsUnicode(false);
            entity.Property(e => e.QualificationNumber)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.QualificationNumberNoObliques)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RegulationStartDate).HasColumnType("datetime");
            entity.Property(e => e.ReviewDate).HasColumnType("datetime");
            entity.Property(e => e.Specialism).IsUnicode(false);
            entity.Property(e => e.Ssa)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UiLastUpdatedDate).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
