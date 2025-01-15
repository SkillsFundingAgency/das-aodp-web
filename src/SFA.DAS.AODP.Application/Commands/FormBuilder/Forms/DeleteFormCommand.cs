using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class DeleteFormCommand : IRequest<DeleteFormCommandResponse>
{
    public Guid Id { get; set; }
}

public class DeleteFormCommandResponse : BaseResponse { }