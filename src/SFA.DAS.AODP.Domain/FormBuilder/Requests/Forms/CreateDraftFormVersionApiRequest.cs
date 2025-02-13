using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;

public class CreateDraftFormVersionApiRequest : IPutApiRequest
{
    public readonly Guid FormId;

    public CreateDraftFormVersionApiRequest(Guid formId)
    {
        FormId = formId;
        Data = new object(); //Unused
    }

    public object Data { get; set; }

    public string PutUrl => $"api/forms/{FormId}/new-version";
}