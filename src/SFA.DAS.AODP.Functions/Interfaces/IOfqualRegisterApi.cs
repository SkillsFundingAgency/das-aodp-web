using RestEase;
using SFA.DAS.AODP.Data;
using SFA.DAS.AODP.Models.Qualification;

namespace SFA.DAS.AODP.Functions.Interfaces
{
    public interface IOfqualRegisterApi
    {
        [Header("Ocp-Apim-Subscription-Key")]
        string SubscriptionKey { get; set; }

        //[Get("gov/Qualification/{qualificationsNumber}")]
        //Task<RegisteredQualification> GetPrivateQualificationsAsync([Path] string qualificationNumber);

        [Get("gov/Qualifications")]
        Task<PaginatedResult<RegisteredQualification>> SearchPrivateQualificationsAsync(
            [Query("title")] string title,
            [Query("page")] int pageNumber,
            [Query("limit")] int pageSize,
            [Query("assessmentMethods")] string assessmentMethods,
            [Query("gradingTypes")] string gradingTypes,
            [Query("awardingOrganisations")] string awardingOrganisations,
            [Query("availability")] string availability,
            [Query("qualificationTypes")] string qualificationTypes,
            [Query("qualificationLevels")] string qualificationLevels,
            [Query("nationalAvailability")] string nationalAvailability,
            [Query("sectorSubjectAreas")] string sectorSubjectAreas,
            [Query("minTotalQualificationTime")] int? minTotalQualificationTime,
            [Query("maxTotalQualificationTime")] int? maxTotalQualificationTime,
            [Query("minGuidedLearninghours")] int? minGuidedLearninghours,
            [Query("maxGuidedLearninghours")] int? maxGuidedLearninghours
        );
    }
}

