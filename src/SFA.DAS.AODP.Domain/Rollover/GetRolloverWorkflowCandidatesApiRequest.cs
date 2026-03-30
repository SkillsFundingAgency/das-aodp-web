using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover;

public class GetRolloverWorkflowCandidatesApiRequest : IGetApiRequest
{
    public string GetUrl => "api/rollover/rolloverworkflowcandidates";
}