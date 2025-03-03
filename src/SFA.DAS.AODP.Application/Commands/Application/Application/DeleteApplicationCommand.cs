using MediatR;
using SFA.DAS.AODP.Application;

public class DeleteApplicationCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationId { get; set; }

    public DeleteApplicationCommand(Guid applicationId)
    {
        ApplicationId = applicationId;
    }
}

public class SubmitApplicationCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationId { get; set; }
    public string SubmittedBy { get; set; }
    public string SubmittedByEmail { get; set; }
}