using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Application;

public class MarkAllMessagesAsReadApiRequest : IPutApiRequest
{
    public Guid ApplicationId { get; set; }
    public string PutUrl => $"api/applications/{ApplicationId}/messages/read";
    public object Data { get; set; }
}
