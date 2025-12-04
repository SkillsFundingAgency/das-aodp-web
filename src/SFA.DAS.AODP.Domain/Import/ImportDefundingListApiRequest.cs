using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Import;

public class ImportDefundingListApiRequest : IPostApiRequest
{
    public string PostUrl => "/api/import/defunding-list";

    public required object Data { get; set; }
}