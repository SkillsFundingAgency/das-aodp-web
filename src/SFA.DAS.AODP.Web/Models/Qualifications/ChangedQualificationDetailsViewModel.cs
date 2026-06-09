using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Enums;
using System.ComponentModel;

namespace SFA.DAS.AODP.Web.Models.Qualifications;

[ExcludeFromCodeCoverage]
public class ChangedQualificationDetailsViewModel
{
    public Guid Id { get; set; }
    public Guid QualificationId { get; set; }
    public int AdditionalKeyChangesReceivedFlag { get; set; }
    public Guid LifecycleStageId { get; set; }
    public string QualificationReference { get; set; } = string.Empty;
    public string AwardingOrganisationName { get; set; } = string.Empty;
    public string QualificationTitle { get; set; } = string.Empty;
    public string QualificationType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string SectorSubjectArea { get; set; } = string.Empty;
    public string? AgeGroup { get; set; }
    public string Name { get; set; } = null!;
    public string? ChangedFieldNames { get; set; }
    public string? OutcomeJustificationNotes { get; set; }
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
    public DateTime OperationalStartDate { get; set; }
    public DateTime? OperationalEndDate { get; set; }
    public DateTime? CertificationEndDate { get; set; }
    public DateTime? ReviewDate { get; set; }
    public bool OfferedInEngland { get; set; }
    public bool OfferedInNi { get; set; }
    public bool? OfferedInternationally { get; set; }
    public bool? IntentionToSeekFundingInEngland { get; set; }
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
    public bool? PreSixteen { get; set; }
    public bool? SixteenToEighteen { get; set; }
    public bool? EighteenPlus { get; set; }
    public bool? NineteenPlus { get; set; }
    public string? ImportStatus { get; set; }
    public bool? EligibleForFunding { get; set; }
    public string? FundingEligibilityFailedFields { get; set; }
    public string EligibilityStatus { get; set; } = null!;
    public EligibleForFundingStatus? EligibleForFundingStatus { get; set; }
    public LifecycleStage? LifecycleStage { get; set; }
    public AwardingOrganisation Organisation { get; set; } = null!;
    public Qualification Qual { get; set; } = null!;
    public ProcessStatusLookup CurrentProcessStatus { get; set; } = null!;
    public AdditionalFormActions AdditionalActions { get; set; } = new();
    public List<ProcessStatus> ProcessStatuses { get; set; } = [];
    public List<OfferFundingDetails> FundingDetails { get; set; } = [];
    public bool? FundingsOffersOutcomeStatus { get; set; }
    public List<ApplicationModel> Applications { get; set; } = [];

    public bool IsQualificationCompleted => string.Equals(LifecycleStage?.Name, "Completed", StringComparison.OrdinalIgnoreCase);

    public KeyFieldPriority Priority => CalculatePriority();

    private List<KeyFieldChange>? _keyFieldChanges;

    public List<KeyFieldChange> GetKeyFieldChanges()
    {
        if (_keyFieldChanges is not null)
        {
            return _keyFieldChanges;
        }

        if (string.IsNullOrWhiteSpace(ChangedFieldNames))
        {
            _keyFieldChanges = [];
            return _keyFieldChanges;
        }

        var changedFields = ChangedFieldNames
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        _keyFieldChanges = KeyField.All
            .Where(kf => changedFields.Any(kf.Matches))
            .Select(kf => new KeyFieldChange(kf, null, null))
            .OrderByDescending(kfc => kfc.GetPriority().Rank)
            .ToList();

        return _keyFieldChanges;
    }

    public string? KeyFieldsForDisplay
    {
        get
        {
            var keyChanges = GetKeyFieldChanges();
            return keyChanges.Count > 0 ? string.Join(", ", keyChanges.Select(o => o.KeyField.DisplayName)) : null;
        }
    }

    private KeyFieldPriority CalculatePriority()
    {
        if (Status == ActionTypeEnum.NoActionRequired)
        {
            return KeyFieldPriority.Green;
        }

        var keyChanges = GetKeyFieldChanges();
        if (keyChanges.Count == 0)
        {
            return KeyFieldPriority.Green;
        }

        var maxPriority = keyChanges.Max(kfc => kfc.GetPriority().Rank);

        if (maxPriority >= KeyFieldPriority.Red.Rank)
        {
            return KeyFieldPriority.Red;
        }

        if (maxPriority >= KeyFieldPriority.Yellow.Rank)
        {
            return KeyFieldPriority.Yellow;
        }

        return KeyFieldPriority.Green;
    }

    internal void MapFundedOffers(GetFeedbackForQualificationFundingByIdQueryResponse feedbackForQualificationFunding)
    {
        foreach (var offer in feedbackForQualificationFunding.QualificationFundedOffers)
        {
            FundingDetails.Add(new()
            {
                FundingOfferName = offer.FundedOfferName,
                StartDate = offer.StartDate,
                EndDate = offer.EndDate
            });
        }
    }

    public static ChangedQualificationDetailsViewModel MapToView(GetQualificationDetailsQueryResponse entity)
    {
        return new ChangedQualificationDetailsViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            QualificationId = entity.QualificationId,
            AdditionalKeyChangesReceivedFlag = entity.AdditionalKeyChangesReceivedFlag,
            LifecycleStageId = entity.LifecycleStageId,
            ChangedFieldNames = entity.VersionFieldChanges,
            OutcomeJustificationNotes = entity.OutcomeJustificationNotes,
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
            OfferedInNi = entity.OfferedInNi,
            OfferedInternationally = entity.OfferedInternationally,
            IntentionToSeekFundingInEngland = entity.IntentionToSeekFundingInEngland, 
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
            PreSixteen = entity.PreSixteen,
            SixteenToEighteen = entity.SixteenToEighteen,
            EighteenPlus = entity.EighteenPlus,
            NineteenPlus = entity.NineteenPlus,
            ImportStatus = entity.ImportStatus,
            EligibleForFunding = entity.EligibleForFunding,
            FundingEligibilityFailedFields = entity.FundingEligibilityFailedFields,
            EligibleForFundingStatus = new EligibleForFundingStatus(entity.EligibleForFunding ?? false, entity.FundingEligibilityFailedFields),
            LifecycleStage = new LifecycleStage
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
                Versions = entity.Qual.Versions.Select(i => new ChangedQualificationDetailsViewModel
                {
                    Id = i.Id,
                    QualificationId = i.QualificationId,
                    Name = i.Name,
                    AdditionalKeyChangesReceivedFlag = i.AdditionalKeyChangesReceivedFlag,
                    LifecycleStageId = i.LifecycleStageId,
                    ChangedFieldNames = i.VersionFieldChanges,
                    OutcomeJustificationNotes = i.OutcomeJustificationNotes,
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
                    IntentionToSeekFundingInEngland = i.IntentionToSeekFundingInEngland,
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
                    PreSixteen = i.PreSixteen,
                    SixteenToEighteen = i.SixteenToEighteen,
                    EighteenPlus = i.EighteenPlus,
                    NineteenPlus = i.NineteenPlus,
                    ImportStatus = i.ImportStatus,
                    EligibleForFunding = i.EligibleForFunding,
                    EligibleForFundingStatus = new EligibleForFundingStatus(i.EligibleForFunding ?? false, i.FundingEligibilityFailedFields),
                    FundingEligibilityFailedFields = i.FundingEligibilityFailedFields,
                    LifecycleStage = new LifecycleStage
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
                    },
                }).ToList()
            },
            CurrentProcessStatus = ProcessStatusLookup.FromName(entity.ProcStatus.Name!)

        };
    }
}

public class OfferFundingDetails
{
    public string FundingOfferName { get; set; } = null!;

    [DisplayName("Start date")]
    public DateOnly? StartDate { get; set; }

    [DisplayName("End date")]
    public DateOnly? EndDate { get; set; }
}

public class LifecycleStage
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}

public class AwardingOrganisation
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

public class Qualification
{
    public Guid Id { get; set; }
    public string Qan { get; set; } = null!;
    public string? QualificationName { get; set; }

    public List<ChangedQualificationDetailsViewModel> Versions { get; set; } = [];
}

public class ActionType
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
}

public class AdditionalFormActions
{
    [HtmlAttributeName("comment")]
    public string Note { get; set; } = string.Empty;
    public Guid? ProcessStatusId { get; set; }
}