using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

public class GetAllSectionsQuery : IRequest<GetAllSectionsQueryResponse>
{
    public Guid FormId { get; set; }
}

public class GetAllSectionsQueryResponse : BaseResponse
{
    public List<Section> Data { get; set; }
}