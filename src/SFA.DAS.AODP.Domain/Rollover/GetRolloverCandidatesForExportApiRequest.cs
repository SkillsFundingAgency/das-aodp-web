using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public class GetRolloverCandidatesForExportApiRequest : IGetApiRequest
{
    public Guid RolloverWorkflowRunId { get; set; }
    public string GetUrl => $"api/rollover/{RolloverWorkflowRunId}/rollovercandidatesforexport";
}