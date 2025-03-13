using MediatR;

namespace SFA.DAS.AODP.Application.Commands.Application.Application;

public class CreateApplicationMessageCommand : IRequest<BaseMediatrResponse<CreateApplicationMessageCommandResponse>>
{
    public Guid ApplicationId { get; set; }
    public string MessageText { get; set; }
    public string MessageType { get; set; }
    public string UserType { get; set; }
    public string SentByEmail { get; set; }
    public string SentByName { get; set; }

    public CreateApplicationMessageCommand(Guid applicationId, string messageText, string messageType, string userType, string sentByEmail, string sentByName)
    {
        ApplicationId = applicationId;
        MessageText = messageText;
        MessageType = messageType;
        UserType = userType;
        SentByEmail = sentByEmail;
        SentByName = sentByName;
    }
}
