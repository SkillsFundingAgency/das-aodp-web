using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover;

public class GetRolloverCandidatesForExportApiRequest : IGetApiRequest
{
    public Guid RolloverWorkflowRunId { get; set; }
    public string GetUrl => $"api/rollover/{RolloverWorkflowRunId}/rollovercandidatesforexport";
}