using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Application.Application;

public class GetApplicationMessagesByIdQuery : IRequest<BaseMediatrResponse<GetApplicationMessagesByIdQueryResponse>>
{
    public Guid ApplicationId { get; set; }
    public string? UserType { get; set; }
    public GetApplicationMessagesByIdQuery(Guid applicationId, string userType)
    {
        ApplicationId = applicationId;
        UserType = userType;
    }
    public GetApplicationMessagesByIdQuery(Guid applicationId)
    {
        ApplicationId = applicationId;
    }
}