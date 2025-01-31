using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;

namespace SFA.DAS.AODP.Web.Models.Routing
{
    public class ListRoutesViewModel
    {
        public GetRoutingInformationForFormQueryResponse Response { get; set; }
        public Guid FormVersionId { get; set; }
    }
}
