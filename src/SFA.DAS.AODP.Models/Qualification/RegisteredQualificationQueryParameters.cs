namespace SFA.DAS.AODP.Models.Qualification;

public class RegisteredQualificationQueryParameters
{
    public string Title { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string AssessmentMethods { get; set; }
    public string GradingTypes { get; set; }
    public string AwardingOrganisations { get; set; }
    public string Availability { get; set; }
    public string QualificationTypes { get; set; }
    public string QualificationLevels { get; set; }
    public string NationalAvailability { get; set; }
    public string SectorSubjectAreas { get; set; }
    public int? MinTotalQualificationTime { get; set; }
    public int? MaxTotalQualificationTime { get; set; }
    public int? MinGuidedLearningHours { get; set; }
    public int? MaxGuidedLearningHours { get; set; }
}

