using SFA.DAS.AODP.Application.Queries.Application.Review;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    [ExcludeFromCodeCoverage]
    public class ApplicationExportViewModel
    {
        public ApplicationReadOnlyDetailsSummary ApplicationSummaryModel { get; set; }
        public ApplicationReadOnlyDetailsViewModel ApplicationFormModel { get; set; }
        public Dictionary<Guid, string> QuestionMap { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ApplicationReadOnlyDetailsSummary
    {

        public string SubmissionId { get; set; } = string.Empty;          
        public string QualificationTitle { get; set; } = string.Empty;    
        public string OrganisationName { get; set; } = string.Empty;      
        public string FormName { get; set; } = string.Empty;             
        public string? Qan { get; set; }

        public ApplicationReadOnlyDetailsSummary(ApplicationExportMetadataResponse response)
        { 
            SubmissionId = response.SubmissionId.ToString();
            QualificationTitle = response.QualificationTitle;
            OrganisationName = response.OrganisationName ?? string.Empty;
            FormName = response.FormName;
            Qan = response.Qan;
        }
    }
}
