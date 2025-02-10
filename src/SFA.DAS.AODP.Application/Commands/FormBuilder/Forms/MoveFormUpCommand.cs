using MediatR;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class MoveFormUpCommand : IRequest<BaseMediatrResponse<MoveFormUpCommandResponse>>
{
    public readonly Guid FormVersionId;

    public MoveFormUpCommand(Guid formVersionId)
    {
        FormVersionId = formVersionId;
    }
}
