using MediatR;
using SFA.DAS.AODP.Application;

public class GetApplicationFormByIdQuery : IRequest<BaseMediatrResponse<GetApplicationFormByIdQueryResponse>>
{
    public GetApplicationFormByIdQuery(Guid formVersionId)
    {
        FormVersionId = formVersionId;
    }
    public Guid FormVersionId { get; set; }

}
