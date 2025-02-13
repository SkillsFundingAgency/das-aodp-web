using SFA.DAS.AODP.Domain.Interfaces;
namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;

public class CreateFormVersionApiRequest : IPostApiRequest
{
    public string PostUrl => "api/forms";

    public object Data { get; set; }

}