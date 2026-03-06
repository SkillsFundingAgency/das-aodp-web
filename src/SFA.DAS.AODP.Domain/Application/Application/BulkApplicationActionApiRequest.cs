using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Application;

public class BulkApplicationActionApiRequest : IPutApiRequest
{
    public Guid ApplicationId { get; set; }
    public string PutUrl => "/api/application-reviews/bulk-action";
    public object Data { get; set; }

}

