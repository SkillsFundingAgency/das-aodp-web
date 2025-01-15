using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class CreateSectionCommand : IRequest<CreateSectionCommandResponse>
{
    public Guid FormId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int? NextSectionId { get; set; }
}

public class CreateSectionCommandResponse : BaseResponse { }