using MediatR;
namespace SFA.DAS.AODP.Application.Queries.Rollover
{
    public class GetRolloverCandidatesForExportQuery : IRequest<BaseMediatrResponse<GetRolloverCandidatesForExportQueryResponse>>
    {
        public Guid RolloverWorkflowRunId { get; set; }
    }
}