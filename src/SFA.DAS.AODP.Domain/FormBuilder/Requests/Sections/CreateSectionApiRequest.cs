using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;

public class CreateSectionApiRequest : IPostApiRequest
{
    public Guid FormVersionId { get; set; }

    public string PostUrl => $"/api/forms/{FormVersionId}/sections";

    public object Data { get; set; }
}