using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class DeletePageCommand : IRequest<DeletePageCommandResponse>
{
    public Guid Id { get; set; }
}

public class DeletePageCommandResponse : BaseResponse { }