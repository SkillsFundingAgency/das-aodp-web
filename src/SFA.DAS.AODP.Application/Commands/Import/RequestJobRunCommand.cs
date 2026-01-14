using MediatR;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class RequestJobRunCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public string JobName { get; set; } // Job Name. Made this change to create the PR as requested.
    public string UserName { get; set; }
}
