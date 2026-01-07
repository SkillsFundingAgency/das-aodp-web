using MediatR;
namespace SFA.DAS.AODP.Application.Commands.Application.Application;
public class SubmitApplicationCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationId { get; set; }
    public required string SubmittedBy { get; set; }
    public required string SubmittedByEmail { get; set; }
}