using MediatR;
using SFA.DAS.AODP.Application;

public class GetApplicationPageByIdQuery : IRequest<BaseMediatrResponse<GetApplicationPageByIdQueryResponse>>
{
    public int PageOrder { get; set; }
    public Guid SectionId { get; set; }
    public Guid FormVersionId { get; set; }

}
