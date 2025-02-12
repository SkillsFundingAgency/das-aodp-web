using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Routes;

public class GetAvailableSectionsAndPagesForRoutingApiRequest : IGetApiRequest
{
    public Guid FormVersionId { get; set; }


    public string GetUrl => $"api/routes/forms/{FormVersionId}/available-sections";
}
