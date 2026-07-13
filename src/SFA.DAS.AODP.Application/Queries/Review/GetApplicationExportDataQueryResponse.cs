
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Application.Queries.Review;
using SFA.DAS.AODP.Domain.Application.Review;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Queries.Application.Review
{
    [ExcludeFromCodeCoverage]
    public class GetApplicationExportDataQueryResponse
    {
        public GetFormPreviewByIdQueryResponse ApplicationFormStructure { get; set; }

        public GetApplicationFormByReviewIdQueryResponse ApplicationFormResponse { get; set; }

        public ApplicationExportMetadataResponse ApplicationMetadata { get; set; } 
    }

    [ExcludeFromCodeCoverage]
    public class ApplicationExportMetadataResponse
    {
        public int SubmissionId { get; set; }
        public string QualificationTitle { get; set; } = string.Empty;
        public string OrganisationName { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;
        public string? Qan { get; set; } = string.Empty;
    }
}
