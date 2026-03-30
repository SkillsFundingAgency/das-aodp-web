using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Models.Rollover;

namespace SFA.DAS.AODP.Application.Validators
{
    public static class RolloverUploadQualificationsValidator
    {
        public static FundingExtensionCandidateValidation Validate(IEnumerable<FundingExtensionCandidate> candidates,
            IEnumerable<RolloverCandidate> rolloverCandidates,
            IEnumerable<RolloverWorkflowCandidate> rolloverWorkflowCandidates,
            RolloverWorkflowRun rolloverRun)
        {
            var response = new FundingExtensionCandidateValidation();

            try
            {
                foreach (var item in candidates)
                {
                    var rc = rolloverCandidates.Where(x => x.QualificationNumber == item.Qan).FirstOrDefault();
                    if (rc == null)
                    {
                        response.AddError(item.RowNumber, "This candidate is no longer viable for RollOver (Candidate Qualification is not currently Approved)");
                        break;
                    }

                    // to do Qual Status is not On Hold or Decision Required

                    var rwc = rolloverWorkflowCandidates.Where(x => x.RolloverCandidatesId == rc.Id).FirstOrDefault();
                    if (rwc == null)
                    {
                        response.AddError(item.RowNumber, "This candidate has been added to the file after P1 Analysis");
                        break;
                    }

                    // to do
                }
            }
            catch (Exception)
            {
                throw;
            }

            return response;
        }
    }
}