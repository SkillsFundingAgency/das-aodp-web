using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;

public class DeleteFormVersionApiRequest : IDeleteApiRequest
{
    public readonly Guid FormVersionId;

    public DeleteFormVersionApiRequest(Guid formVersionId)
    {
        FormVersionId = formVersionId;
    }

    public string DeleteUrl => $"/api/forms/{FormVersionId}";
}