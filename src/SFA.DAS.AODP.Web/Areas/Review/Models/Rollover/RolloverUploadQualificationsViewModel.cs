using SFA.DAS.AODP.Web.Views.Shared;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    [ExcludeFromCodeCoverage]
    public class RolloverUploadQualificationsViewModel
    {
        [Required(ErrorMessage = "You must select a CSV file.")]
        public IFormFile File { get; set; }

        public RolloverUploadPageState State { get; set; }

        public RolloverUploadValidationResult? ValidationResult { get; set; } = new();

        private static readonly BackLinkModel _backLinkModel = new()
        {
            Action = "Index",
            Controller = "Rollover",
            Area = "Review"
        };

        public BackLinkModel BackLinkModel => _backLinkModel;

    }

    [ExcludeFromCodeCoverage]
    public class RolloverUploadValidationResult
    {
        public bool IsValid { get; set; }

        public int TotalCandidates { get; set; }

        public int FailedCandidateCount { get; set; }

        public string? ErrorFileToken { get; set; }
    }

    public enum RolloverUploadPageState
    {
        Empty,          // initial upload view
        FileError,      // CSV parsing/file format error
        ValidationFailed, // business validation failed
        ReadyToSubmit   // valid file, can proceed
    }
}