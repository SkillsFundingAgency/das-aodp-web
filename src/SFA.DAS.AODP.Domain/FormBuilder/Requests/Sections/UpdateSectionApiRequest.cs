using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Sections;

public class UpdateSectionApiRequest : IPutApiRequest
{
    public Guid FormVersionId { get; set; }
    public Guid SectionId { get; set; }


    public string PutUrl => $"api/forms/{FormVersionId}/sections/{SectionId}";

    public object Data { get; set; }

}