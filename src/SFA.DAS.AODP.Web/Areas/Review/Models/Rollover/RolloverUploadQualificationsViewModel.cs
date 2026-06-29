using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Controllers;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    [ExcludeFromCodeCoverage]
    public class RolloverUploadQualificationsViewModel
    {
        [Required(ErrorMessage = "You must select a CSV file.")]
        public IFormFile File { get; set; }

        public RolloverValidationErrorViewModel? ValidationSummary { get; set; }

        public string ReturnViewName { get; set; } = nameof(RolloverController.UploadQualificationsToRollover);

    }

    [ExcludeFromCodeCoverage]
    public class RolloverValidationErrorViewModel
    {
        public int FailedCandidateCount { get; set; }

        public string? ErrorFileToken { get; set; }
        public List<RolloverValidationErrorItem> NotValidCandidates { get; set; } = new();

    }

    [ExcludeFromCodeCoverage]
    public class RolloverSummaryViewModel
    {
        public int TotalCandidatesCount { get; set; }

        public int CandidatesExtendedInUploadCount { get; set; }

        public int TotalCandidatesToBeExtendedCount { get; set; }

        public int TotalCandidatesToBeExcludedCount { get; set; }

        public int TotalCandidatesToBeReviewedCount { get; set; }

        public RolloverSummaryViewModel() { }
        public RolloverSummaryViewModel(FundingExtensionSummary summaryFromResponse)
        {
            TotalCandidatesCount = summaryFromResponse.TotalCandidatesCount;
            CandidatesExtendedInUploadCount = summaryFromResponse.CandidatesExtendedInUploadCount;
            TotalCandidatesToBeExtendedCount = summaryFromResponse.TotalCandidatesToBeExtendedCount;
            TotalCandidatesToBeExcludedCount = summaryFromResponse.TotalCandidatesToBeExcludedCount;
            TotalCandidatesToBeReviewedCount = summaryFromResponse.TotalCandidatesToBeReviewedCount;
        }

    }

    [ExcludeFromCodeCoverage]
    public class RolloverValidationErrorItem
    {
        public string Qan { get; set; } = string.Empty;
        public string FundingStream { get; set; } = string.Empty;
        public List<string> ErrorMessages { get; set; } = new();
    }
}