namespace SFA.DAS.AODP.Application.Commands.Application.Application;
public class EditApplicationCommandResponse 
{
    public bool? IsQanValid { get; set; } = false;
    public string? QanValidationMessage { get; set; }
}