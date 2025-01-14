namespace SFA.DAS.AODP.Models.Qualification;

public partial class RegisteredQualification
{
    public int Id { get; set; }

    public string QualificationNumber { get; set; } = null!;

    public string? QualificationNumberNoObliques { get; set; }

    public string Title { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string OrganisationName { get; set; } = null!;

    public string OrganisationAcronym { get; set; } = null!;

    public string OrganisationRecognitionNumber { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Ssa { get; set; } = null!;

    public string Level { get; set; } = null!;

    public string SubLevel { get; set; } = null!;

    public string EqfLevel { get; set; }

    public string? GradingType { get; set; }

    public string? GradingScale { get; set; }

    public int TotalCredits { get; set; }

    public int? Tqt { get; set; }

    public int? Glh { get; set; }

    public int MinimumGlh { get; set; }

    public int MaximumGlh { get; set; }

    public DateTime RegulationStartDate { get; set; }

    public DateTime OperationalStartDate { get; set; }

    public DateTime OperationalEndDate { get; set; }

    public DateTime CertificationEndDate { get; set; }

    public DateTime ReviewDate { get; set; }

    public bool OfferedInEngland { get; set; }

    public bool OfferedInNorthernIreland { get; set; }

    public bool? OfferedInternationally { get; set; }

    public string? Specialism { get; set; }

    public string? Pathways { get; set; }

    public Object? AssessmentMethods { get; set; }

    public string? ApprovedForDelfundedProgramme { get; set; }

    public string? LinkToSpecification { get; set; }

    public string? ApprenticeshipStandardReferenceNumber { get; set; }

    public string? ApprenticeshipStandardTitle { get; set; }

    public bool RegulatedByNorthernIreland { get; set; }

    public string? NiDiscountCode { get; set; }

    public string? GceSizeEquivalence { get; set; }

    public string? GcseSizeEquivalence { get; set; }

    public string? EntitlementFrameworkDesignation { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    public DateTime UiLastUpdatedDate { get; set; }

    public DateTime InsertedDate { get; set; }

    public int? Version { get; set; }

    public bool? AppearsOnPublicRegister { get; set; }

    public int? OrganisationId { get; set; }

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

    //public string? ChangedFields { get; set; }
}
