using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

public class GetPageByIdQuery : IRequest<GetPageByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class GetPageByIdQueryResponse : BaseResponse
{
    public Page Data { get; set; }
}