namespace SFA.DAS.AODP.Application.Commands.Application.Application;

public class CreateApplicationMessageCommandResponse
{
    public Guid Id { get; set; }
    public bool EmailSent { get; set; } = false;
}