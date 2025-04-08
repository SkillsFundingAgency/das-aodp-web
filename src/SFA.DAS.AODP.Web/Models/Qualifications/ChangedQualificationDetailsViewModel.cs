using Microsoft.AspNetCore.Razor.TagHelpers;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Qualifications;

public class ChangedQualificationDetailsViewModel
{
    public Guid Id { get; set; }
    public Guid QualificationId { get; set; }
    public Guid VersionFieldChangesId { get; set; }
    public Guid ProcessStatusId { get; set; }
    public int AdditionalKeyChangesReceivedFlag { get; set; }
    public string? VersionFieldChanges { get; set; }
    public Guid LifecycleStageId { get; set; }
    public string? ChangedFieldNames { get; set; }
    public string? OutcomeJustificationNotes { get; set; }
    public Guid AwardingOrganisationId { get; set; }
    public string Status { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Ssa { get; set; } = null!;
    public string Level { get; set; } = null!;
    public string SubLevel { get; set; } = null!;
    public string EqfLevel { get; set; } = null!;
    public string? GradingType { get; set; }
    public string? GradingScale { get; set; }
    public int? TotalCredits { get; set; }
    public int? Tqt { get; set; }
    public int? Glh { get; set; }
    public int? MinimumGlh { get; set; }
    public int? MaximumGlh { get; set; }
    public DateTime RegulationStartDate { get; set; }
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]

    public DateTime OperationalStartDate { get; set; }

    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]

    public DateTime? OperationalEndDate { get; set; }
    public DateTime? CertificationEndDate { get; set; }
    public DateTime? ReviewDate { get; set; }
    public bool OfferedInEngland { get; set; }
    public bool? FundedInEngland { get; set; }
    public bool OfferedInNi { get; set; }
    public bool? OfferedInternationally { get; set; }
    public string? Specialism { get; set; }
    public string? Pathways { get; set; }
    public string? AssessmentMethods { get; set; }
    public string? ApprovedForDelFundedProgramme { get; set; }
    public string? LinkToSpecification { get; set; }
    public string? ApprenticeshipStandardReferenceNumber { get; set; }
    public string? ApprenticeshipStandardTitle { get; set; }
    public bool RegulatedByNorthernIreland { get; set; }
    public string? NiDiscountCode { get; set; }
    public string? GceSizeEquivelence { get; set; }
    public string? GcseSizeEquivelence { get; set; }
    public string? EntitlementFrameworkDesign { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public DateTime UiLastUpdatedDate { get; set; }
    public DateTime InsertedDate { get; set; }
    public DateTime? InsertedTimestamp { get; set; }
    public int? Version { get; set; }
    public bool? AppearsOnPublicRegister { get; set; }
    public int? LevelId { get; set; }
    public int? TypeId { get; set; }
    public int? SsaId { get; set; }
    public int? GradingTypeId { get; set; }
    public int? GradingScaleId { get; set; }
    public bool? PreSixteen { get; set; }
    public bool? SixteenToEighteen { get; set; }
    public bool? EighteenPlus { get; set; }
    public bool? NineteenPlus { get; set; }
    public string? ImportStatus { get; set; }
    public virtual LifecycleStage Stage { get; set; } = null!;
    public virtual AwardingOrganisation Organisation { get; set; } = null!;
    public virtual Qualification Qual { get; set; } = null!;
    public virtual ProcessStatus ProcStatus { get; set; } = null!;
    public AdditionalFormActions AdditionalActions { get; set; } = new AdditionalFormActions();
    public List<ProcessStatus> ProcessStatuses { get; set; } = new List<ProcessStatus>();
    public List<OfferFundingDetails> Details { get; set; } = new();
    public List<FundingOffer> FundingOffers { get; set; } = new();
    public class OfferFundingDetails
    {
        public Guid FundingOfferId { get; set; }
        [DisplayName("Start date")]
        public DateOnly? StartDate { get; set; }
        [DisplayName("End date")]
        public DateOnly? EndDate { get; set; }
        public string? Comments { get; set; }
    }

    public class FundingOffer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    public string Priority
    {
        get
        {
            return MapChangedFieldsToPriority();
        }
    }

    private string MapChangedFieldsToPriority()
    {
        var priority = "Green";
        if (!string.IsNullOrWhiteSpace(priority) && Status != ActionTypeEnum.NoActionRequired)
        {
            var changedFields = ChangedFieldNames.Split(',').Select(s => s.Trim()).ToList();
            var redChanges = new List<string>() { "Level", "SSA", "GLH" };
            var yellowChanges = new List<string>()
                {
                    "OrganisationName",
                    "Title",
                    "Type",
                    "TotalCredits",
                    "GradingType",
                    "OfferedInEngland",
                    "PreSixteen",
                    "SixteenToEighteen",
                    "EighteenPlus",
                    "NineteenPlus",
                    "MinimumGLH",
                    "TQT",
                    "OperationalEndDate",
                    "LastUpdatedDate",
                    "Version",
                    "OfferedInternationally"
                };
            if (changedFields.Intersect(redChanges).Any())
            {
                priority = "Red";
            }
            else if (changedFields.Intersect(yellowChanges).Any())
            {
                priority = "Yellow";
            }

        }
        return priority;
    }

    public List<KeyFieldChanges> KeyFieldChanges { get; set; } = new();
    public partial class LifecycleStage
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public partial class AwardingOrganisation
    {

        public Guid Id { get; set; }
        public int? Ukprn { get; set; }
        public string? RecognitionNumber { get; set; }
        public string? NameLegal { get; set; }
        public string? NameOfqual { get; set; }
        public string? NameGovUk { get; set; }
        public string? Name_Dsi { get; set; }
        public string? Acronym { get; set; }
    }

    public partial class Qualification
    {
        public Guid Id { get; set; }
        public string Qan { get; set; } = null!;
        public string? QualificationName { get; set; }

        public List<ChangedQualificationDetailsViewModel> Versions { get; set; }
    }

    public class ActionType
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
    }

    public partial class ProcessStatus
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? IsOutcomeDecision { get; set; }

        public static implicit operator ProcessStatus(GetProcessStatusesQueryResponse.ProcessStatus model)
        {
            return new ProcessStatus
            {
                Id = model.Id,
                Name = model.Name,
                IsOutcomeDecision = model.IsOutcomeDecision,
            };
        }
    }

    public class AdditionalFormActions
    {
        [HtmlAttributeName("comment")]
        public string Note { get; set; } = string.Empty;
        public Guid? ProcessStatusId { get; set; }
    }

    public static implicit operator ChangedQualificationDetailsViewModel(GetQualificationDetailsQueryResponse entity)
    {
        return new ChangedQualificationDetailsViewModel()
        {
            Id = entity.Id,
            QualificationId = entity.QualificationId,
            VersionFieldChangesId = entity.VersionFieldChangesId,
            ProcessStatusId = entity.ProcessStatusId,
            AdditionalKeyChangesReceivedFlag = entity.AdditionalKeyChangesReceivedFlag,
            LifecycleStageId = entity.LifecycleStageId,
            ChangedFieldNames = entity.VersionFieldChanges,
            OutcomeJustificationNotes = entity.OutcomeJustificationNotes,
            AwardingOrganisationId = entity.AwardingOrganisationId,
            Status = entity.Status,
            Type = entity.Type,
            Ssa = entity.Ssa,
            Level = entity.Level,
            SubLevel = entity.SubLevel,
            EqfLevel = entity.EqfLevel,
            GradingType = entity.GradingType,
            GradingScale = entity.GradingScale,
            TotalCredits = entity.TotalCredits,
            Tqt = entity.Tqt,
            Glh = entity.Glh,
            MinimumGlh = entity.MinimumGlh,
            MaximumGlh = entity.MaximumGlh,
            RegulationStartDate = entity.RegulationStartDate,
            OperationalStartDate = entity.OperationalStartDate,
            OperationalEndDate = entity.OperationalEndDate,
            CertificationEndDate = entity.CertificationEndDate,
            ReviewDate = entity.ReviewDate,
            OfferedInEngland = entity.OfferedInEngland,
            FundedInEngland = entity.FundedInEngland,
            OfferedInNi = entity.OfferedInNi,
            OfferedInternationally = entity.OfferedInternationally,
            Specialism = entity.Specialism,
            Pathways = entity.Pathways,
            AssessmentMethods = entity.AssessmentMethods,
            ApprovedForDelFundedProgramme = entity.ApprovedForDelFundedProgramme,
            LinkToSpecification = entity.LinkToSpecification,
            ApprenticeshipStandardReferenceNumber = entity.ApprenticeshipStandardReferenceNumber,
            ApprenticeshipStandardTitle = entity.ApprenticeshipStandardTitle,
            RegulatedByNorthernIreland = entity.RegulatedByNorthernIreland,
            NiDiscountCode = entity.NiDiscountCode,
            GceSizeEquivelence = entity.GceSizeEquivelence,
            GcseSizeEquivelence = entity.GcseSizeEquivelence,
            EntitlementFrameworkDesign = entity.EntitlementFrameworkDesign,
            LastUpdatedDate = entity.LastUpdatedDate,
            UiLastUpdatedDate = entity.UiLastUpdatedDate,
            InsertedDate = entity.InsertedDate,
            InsertedTimestamp = entity.InsertedTimestamp,
            Version = entity.Version,
            AppearsOnPublicRegister = entity.AppearsOnPublicRegister,
            LevelId = entity.LevelId,
            TypeId = entity.TypeId,
            SsaId = entity.SsaId,
            GradingTypeId = entity.GradingTypeId,
            GradingScaleId = entity.GradingScaleId,
            PreSixteen = entity.PreSixteen,
            SixteenToEighteen = entity.SixteenToEighteen,
            EighteenPlus = entity.EighteenPlus,
            NineteenPlus = entity.NineteenPlus,
            ImportStatus = entity.ImportStatus,
            Stage = new LifecycleStage
            {
                Id = entity.Stage.Id,
                Name = entity.Stage.Name,
            },
            Organisation = new AwardingOrganisation
            {
                Id = entity.Organisation.Id,
                Ukprn = entity.Organisation.Ukprn,
                RecognitionNumber = entity.Organisation.RecognitionNumber,
                NameLegal = entity.Organisation.NameLegal,
                NameOfqual = entity.Organisation.NameOfqual,
                NameGovUk = entity.Organisation.NameGovUk,
                Name_Dsi = entity.Organisation.Name_Dsi,
                Acronym = entity.Organisation.Acronym,
            },
            Qual = new Qualification
            {
                Id = entity.Qual.Id,
                Qan = entity.Qual.Qan,
                QualificationName = entity.Qual.QualificationName,
                Versions = (List<ChangedQualificationDetailsViewModel>)entity.Qual.Versions.Select(i => new ChangedQualificationDetailsViewModel()
                {

                    Id = i.Id,
                    QualificationId = i.QualificationId,
                    VersionFieldChangesId = i.VersionFieldChangesId,
                    ProcessStatusId = i.ProcessStatusId,
                    AdditionalKeyChangesReceivedFlag = i.AdditionalKeyChangesReceivedFlag,
                    LifecycleStageId = i.LifecycleStageId,
                    ChangedFieldNames = i.VersionFieldChanges,
                    OutcomeJustificationNotes = i.OutcomeJustificationNotes,
                    AwardingOrganisationId = i.AwardingOrganisationId,
                    Status = i.Status,
                    Type = i.Type,
                    Ssa = i.Ssa,
                    Level = i.Level,
                    SubLevel = i.SubLevel,
                    EqfLevel = i.EqfLevel,
                    GradingType = i.GradingType,
                    GradingScale = i.GradingScale,
                    TotalCredits = i.TotalCredits,
                    Tqt = i.Tqt,
                    Glh = i.Glh,
                    MinimumGlh = i.MinimumGlh,
                    MaximumGlh = i.MaximumGlh,
                    RegulationStartDate = i.RegulationStartDate,
                    OperationalStartDate = i.OperationalStartDate,
                    OperationalEndDate = i.OperationalEndDate,
                    CertificationEndDate = i.CertificationEndDate,
                    ReviewDate = i.ReviewDate,
                    OfferedInEngland = i.OfferedInEngland,
                    OfferedInNi = i.OfferedInNi,
                    OfferedInternationally = i.OfferedInternationally,
                    Specialism = i.Specialism,
                    Pathways = i.Pathways,
                    AssessmentMethods = i.AssessmentMethods,
                    ApprovedForDelFundedProgramme = i.ApprovedForDelFundedProgramme,
                    LinkToSpecification = i.LinkToSpecification,
                    ApprenticeshipStandardReferenceNumber = i.ApprenticeshipStandardReferenceNumber,
                    ApprenticeshipStandardTitle = i.ApprenticeshipStandardTitle,
                    RegulatedByNorthernIreland = i.RegulatedByNorthernIreland,
                    NiDiscountCode = i.NiDiscountCode,
                    GceSizeEquivelence = i.GceSizeEquivelence,
                    GcseSizeEquivelence = i.GcseSizeEquivelence,
                    EntitlementFrameworkDesign = i.EntitlementFrameworkDesign,
                    LastUpdatedDate = i.LastUpdatedDate,
                    UiLastUpdatedDate = i.UiLastUpdatedDate,
                    InsertedDate = i.InsertedDate,
                    InsertedTimestamp = i.InsertedTimestamp,
                    Version = i.Version,
                    AppearsOnPublicRegister = i.AppearsOnPublicRegister,
                    LevelId = i.LevelId,
                    TypeId = i.TypeId,
                    SsaId = i.SsaId,
                    GradingTypeId = i.GradingTypeId,
                    GradingScaleId = i.GradingScaleId,
                    PreSixteen = i.PreSixteen,
                    SixteenToEighteen = i.SixteenToEighteen,
                    EighteenPlus = i.EighteenPlus,
                    NineteenPlus = i.NineteenPlus,
                    ImportStatus = i.ImportStatus,
                    Stage = new LifecycleStage
                    {
                        Id = i.Stage.Id,
                        Name = i.Stage.Name,
                    },
                    Organisation = new AwardingOrganisation
                    {
                        Id = i.Organisation.Id,
                        Ukprn = i.Organisation.Ukprn,
                        RecognitionNumber = i.Organisation.RecognitionNumber,
                        NameLegal = i.Organisation.NameLegal,
                        NameOfqual = i.Organisation.NameOfqual,
                        NameGovUk = i.Organisation.NameGovUk,
                        Name_Dsi = i.Organisation.Name_Dsi,
                        Acronym = i.Organisation.Acronym,
                    },
                    Qual = new Qualification
                    {
                        Id = entity.Qual.Id,
                        Qan = entity.Qual.Qan,
                        QualificationName = entity.Qual.QualificationName
                    }


                }).ToList()
            },
            ProcStatus = new ProcessStatus()
            {
                Id = entity.ProcStatus.Id,
                Name = entity.ProcStatus.Name,
                IsOutcomeDecision = entity.ProcStatus.IsOutcomeDecision,
            },

        };
    }
}

public class KeyFieldChanges
{
    public string Was { get; set; }
    public string Now { get; set; }
    public string Name { get; set; }
}