using MediatR;
using System.Diagnostics.CodeAnalysis;
namespace SFA.DAS.AODP.Application.Queries.Rollover
{
    [ExcludeFromCodeCoverage]
    public class GetRolloverCandidatesForExportQuery : IRequest<BaseMediatrResponse<GetRolloverCandidatesForExportQueryResponse>>
    {
        public Guid RolloverWorkflowRunId { get; set; }
    }
}