using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

public class GetSectionByIdQuery : IRequest<GetSectionByIdQueryResponse>
{
    public Guid Id { get; set; }
}

public class GetSectionByIdQueryResponse : BaseResponse
{
    public Section Data { get; set; }
}