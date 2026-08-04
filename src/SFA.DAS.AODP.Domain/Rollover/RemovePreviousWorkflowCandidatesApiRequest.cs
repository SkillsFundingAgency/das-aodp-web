using SFA.DAS.AODP.Domain.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Domain.Rollover;

[ExcludeFromCodeCoverage]
public class RemovePreviousWorkflowCandidatesApiRequest : IPostApiRequest
{
    public string PostUrl => "api/rollover/removepreviousworkflowcandidates";
    public object Data { get; set; } = new { };
}
