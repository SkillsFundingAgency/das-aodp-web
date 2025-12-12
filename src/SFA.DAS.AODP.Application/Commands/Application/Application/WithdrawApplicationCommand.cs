using MediatR;
using SFA.DAS.AODP.Application;

public class WithdrawApplicationCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public Guid ApplicationId { get; set; }
    public string WithdrawnBy { get; set; }
    public string WithdrawnByEmail { get; set; }
}