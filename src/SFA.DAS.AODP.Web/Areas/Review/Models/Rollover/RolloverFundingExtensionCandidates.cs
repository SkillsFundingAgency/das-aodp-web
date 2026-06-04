using SFA.DAS.AODP.Models.Rollover;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover
{
    public class RolloverFundingExtensionCandidates
    {
        public List<FundingExtensionCandidate> FundingExtensionCandidates { get; set; } = new();
        public FundingExtensionCandidateValidation? FundingExtensionCandidateValidation { get; set; }
    }
}