using MediatR;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class CreateDraftFormVersionCommand : IRequest<BaseMediatrResponse<CreateDraftFormVersionCommandResponse>>
{
    public readonly Guid FormId;

    public CreateDraftFormVersionCommand(Guid formId)
    {
        FormId = formId;
    }
}
