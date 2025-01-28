using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;

public class UpdateFormVersionApiRequest : IPutApiRequest
{
    public Guid FormVersionId { get; set; }
    public object Data { get; set; }

    public string PutUrl => $"/api/forms/{FormVersionId}";
}