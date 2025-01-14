using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class DeleteFormCommand : IRequest<BaseResponse<bool>>
{
    public Guid Id { get; set; }
}