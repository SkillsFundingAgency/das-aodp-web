using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Import;

public class ImportPldnsApiRequest : IPostApiRequest
{
    public string PostUrl => "/api/import/pldns";

    public required object Data { get; set; }
}