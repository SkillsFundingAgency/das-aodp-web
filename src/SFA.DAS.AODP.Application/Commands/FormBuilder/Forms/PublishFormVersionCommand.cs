using MediatR;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class PublishFormVersionCommand : IRequest<BaseMediatrResponse<PublishFormVersionCommandResponse>>
{
    public readonly Guid FormVersionId;

    public PublishFormVersionCommand(Guid formVersionId)
    {
        FormVersionId = formVersionId;
    }
}
