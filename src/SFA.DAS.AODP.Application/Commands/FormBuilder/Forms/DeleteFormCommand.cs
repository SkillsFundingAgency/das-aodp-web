using MediatR;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class DeleteFormCommand : IRequest<BaseMediatrResponse<EmptyResponse>>
{
    public readonly Guid FormId;

    public DeleteFormCommand(Guid formId)
    {
        FormId = formId;
    }
}