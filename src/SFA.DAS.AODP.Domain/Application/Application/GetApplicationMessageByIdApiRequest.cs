using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Application;

public class GetApplicationMessageByIdApiRequest : IGetApiRequest
{
    public Guid MessageId { get; set; }
    public string GetUrl => $"api/applications/messages/{MessageId}";
}