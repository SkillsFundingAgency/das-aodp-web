using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;

public class CreatePageApiRequest : IPostApiRequest
{
    public Guid FormVersionId { get; set; }
    public Guid SectionId { get; set; }

    public string PostUrl => $"api/forms/{FormVersionId}/sections/{SectionId}/Pages";

    public object Data { get; set; }
}