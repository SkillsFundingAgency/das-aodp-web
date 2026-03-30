namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class FundingExtensionCandidateValidation
    {
        public int ErrorCount { get; set; }
        public List<ErrorDetails> ErrorDetails { get; set; } = new();
    }

    public class ErrorDetails
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}