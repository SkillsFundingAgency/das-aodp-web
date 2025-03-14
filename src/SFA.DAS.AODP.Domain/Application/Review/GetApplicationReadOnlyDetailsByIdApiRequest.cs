using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Review;

public class GetApplicationReadOnlyDetailsByIdApiRequest : IGetApiRequest
{
    public Guid ApplicationId { get; set; }

    public GetApplicationReadOnlyDetailsByIdApiRequest(Guid applicationId)
    {
        ApplicationId = applicationId;
    }

    public string GetUrl => $"api/applications/{ApplicationId}/details";

}
