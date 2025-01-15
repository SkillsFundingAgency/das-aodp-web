using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class DeleteSectionCommand : IRequest<DeleteSectionCommandResponse>
{
    public Guid Id { get; set; }
}

public class DeleteSectionCommandResponse : BaseResponse { }