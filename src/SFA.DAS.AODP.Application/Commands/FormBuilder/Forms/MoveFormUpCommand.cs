using MediatR;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class MoveFormUpCommand : IRequest<BaseMediatrResponse<MoveFormUpCommandResponse>>
{
    public readonly Guid FormId;

    public MoveFormUpCommand(Guid formVersionId)
    {
        FormId = formVersionId;
    }
}
