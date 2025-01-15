using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

public class GetAllPagesQuery : IRequest<GetAllPagesQueryResponse>
{
    public Guid SectionId { get; set; }
}

public class GetAllPagesQueryResponse : BaseResponse
{
    public List<Page> Data { get; set; }
}