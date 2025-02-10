using MediatR;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class MoveFormDownCommand : IRequest<BaseMediatrResponse<MoveFormDownCommandResponse>>
{
    public readonly Guid FormId;

    public MoveFormDownCommand(Guid formVersionId)
    {
        FormId = formVersionId;
    }
}
