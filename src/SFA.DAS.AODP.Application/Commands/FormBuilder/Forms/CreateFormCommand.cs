using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class CreateFormCommand : IRequest<BaseResponse<Form>>
{
    public string Name { get; set; }
    public string Version { get; set; }
    public bool Published { get; set; }
    public string Key { get; set; }
    public string ApplicationTrackingTemplate { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
}