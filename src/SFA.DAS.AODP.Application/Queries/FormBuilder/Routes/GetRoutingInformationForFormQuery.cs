using MediatR;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Routes
{
    public class GetRoutingInformationForFormQuery : IRequest<BaseMediatrResponse<GetRoutingInformationForFormQueryResponse>>
    {
        public Guid FormVersionId { get; set; }
    }
}