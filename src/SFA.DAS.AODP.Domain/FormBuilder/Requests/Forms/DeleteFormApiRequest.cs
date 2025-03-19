using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;

public class DeleteFormApiRequest : IDeleteApiRequest
{
    public readonly Guid FormId;

    public DeleteFormApiRequest(Guid formId)
    {
        FormId = formId;
    }

    public string DeleteUrl => $"api/forms/{FormId}";
}