using MediatR;
using SFA.DAS.AODP.Application;

public class GetApplicationByIdQuery : IRequest<BaseMediatrResponse<GetApplicationByIdQueryResponse>>
{
    public GetApplicationByIdQuery(Guid applicationId)
    {
        ApplicationId = applicationId;
    }
    public Guid ApplicationId { get; set; }
}
