using MediatR;

namespace SFA.DAS.AODP.Application.Commands.Application.Application;

public class CreateApplicationMessageCommand : IRequest<BaseMediatrResponse<CreateApplicationMessageCommandResponse>>
{
    public CreateApplicationMessageCommand(string messageText, Guid applicationId, string userType)
    {
        MessageText = messageText;
        ApplicationId = applicationId;
        UserType = userType;
    }

    public string MessageText { get; set; }
    public Guid ApplicationId { get; set; }
    public string UserType { get; set; }
}
