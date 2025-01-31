using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;

public class GetAvailableQuestionsForRoutingApiRequest : IGetApiRequest
{
    public Guid FormVersionId { get; set; }
    public Guid SectionId { get; set; }
    public Guid PageId { get; set; }


    public string GetUrl => $"api/routes/forms/{FormVersionId}/sections/{SectionId}/pages/{PageId}/available-questions";
}