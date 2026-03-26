namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class RolloverFundingExtensionCandidates
    {
        public List<FundingExtensionCandidate> FundingExtensionCandidatesValid { get; set; } = new();
        public List<FundingExtensionCandidate> FundingExtensionCandidatesInValid { get; set; } = new();
    }
}