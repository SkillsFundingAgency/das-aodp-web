using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Application;

public class CreateApplicationMessageApiRequest : IPostApiRequest
{
    public string PostUrl => "api/TODO";

    public object Data { get; set; }

}
