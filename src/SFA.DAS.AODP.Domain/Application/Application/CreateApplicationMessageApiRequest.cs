using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Application;

public class CreateApplicationMessageApiRequest : IPostApiRequest
{
    public Guid ApplicationId { get; set; }
    public string PostUrl => $"api/applicationMessages/{ApplicationId}/messages";

    public object Data { get; set; }

}
