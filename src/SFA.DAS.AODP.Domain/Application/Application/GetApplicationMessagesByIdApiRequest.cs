using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Application;

public class GetApplicationMessagesByIdApiRequest : IGetApiRequest
{
    public Guid ApplicationId { get; set; }
    public string GetUrl => $"api/applicationMessages/{ApplicationId}/messages";
}