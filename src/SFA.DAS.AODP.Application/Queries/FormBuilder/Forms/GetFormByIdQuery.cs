using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetFormByIdQuery : IRequest<GetFormByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class GetFormByIdQueryResponse : BaseResponse
{
    public Form Data { get; set; }
}