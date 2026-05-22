using MediatR;

namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    public class UpdateRolloverWorkflowCandidatesAfterP1ChecksCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
    { }
}
