using MediatR;

namespace SFA.DAS.AODP.Application.Commands.Application.Application;

public class CreateApplicationMessageCommand : IRequest<BaseMediatrResponse<CreateApplicationMessageCommandResponse>>
{
    public CreateApplicationMessageCommand(string messageText, Guid applicationId)
    {
        MessageText = messageText;
        ApplicationId = applicationId;
    }

    public string MessageText { get; set; }
    public Guid ApplicationId { get; set; }
}
