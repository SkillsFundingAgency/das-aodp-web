using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Application;

public class GetApplicationMessagesByApplicationIdApiRequest : IGetApiRequest
{
    public Guid ApplicationId { get; set; }
    public string UserType { get; set; }
    public string GetUrl => $"api/applications/{ApplicationId}/messages?userType={UserType}";
}
