using MediatR;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class MoveFormDownCommand : IRequest<BaseMediatrResponse<MoveFormDownCommandResponse>>
{
    public readonly Guid FormVersionId;

    public MoveFormDownCommand(Guid formVersionId)
    {
        FormVersionId = formVersionId;
    }
}
