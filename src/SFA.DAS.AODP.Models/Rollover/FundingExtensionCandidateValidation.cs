namespace SFA.DAS.AODP.Models.Rollover
{
    public class FundingExtensionCandidateValidation
    {
        public int ErrorCount { get; set; }
        public List<ErrorDetails> ErrorDetails { get; set; } = new();
    }
}
