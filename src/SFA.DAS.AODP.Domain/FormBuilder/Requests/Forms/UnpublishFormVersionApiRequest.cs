using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;

public class MoveFormDownApiRequest : IPutApiRequest
{
    public readonly Guid FormVersionId;

    public MoveFormDownApiRequest(Guid formVersionId)
    {
        FormVersionId = formVersionId;
        Data = new object(); //Unused
    }

    public object Data { get; set; }

    public string PutUrl => $"api/forms/{FormVersionId}/MoveDown";
}