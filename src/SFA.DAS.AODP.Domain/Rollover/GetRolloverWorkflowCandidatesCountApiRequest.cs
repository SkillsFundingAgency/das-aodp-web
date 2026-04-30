using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public class GetRolloverWorkflowCandidatesCountApiRequest : IGetApiRequest
{
    public string GetUrl => "api/rollover/workflowcandidatescount";
}
