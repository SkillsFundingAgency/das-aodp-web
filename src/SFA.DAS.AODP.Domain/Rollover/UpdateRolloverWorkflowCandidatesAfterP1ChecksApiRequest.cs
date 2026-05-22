using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover
{
    public class UpdateRolloverWorkflowCandidatesAfterP1ChecksApiRequest : IPostApiRequest
    {
        public object Data { get; set; } = new { };

        public string PostUrl => $"api/rollover/p1Checks";
    }
}