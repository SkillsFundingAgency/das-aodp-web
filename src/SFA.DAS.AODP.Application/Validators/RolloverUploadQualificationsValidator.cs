using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Models.Rollover;

namespace SFA.DAS.AODP.Application.Validators
{
    public static class RolloverUploadQualificationsValidator
    {
        public static FundingExtensionCandidateValidation Validate(List<FundingExtensionCandidate> candidates, 
            IEnumerable<RolloverCandidate> rolloverCandidates, 
            IEnumerable<RolloverWorkflowCandidate> rolloverWorkflowCandidates, 
            RolloverWorkflowRun rolloverRun)
        {
            var response = new FundingExtensionCandidateValidation();

            try
            {

            }
            catch (Exception)
            {
                throw;
            }
            
            return response;
        }
    }
}