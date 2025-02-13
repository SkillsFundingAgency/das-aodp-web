using MediatR;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Routes
{
    public class GetAvailableSectionsAndPagesForRoutingQuery : IRequest<BaseMediatrResponse<GetAvailableSectionsAndPagesForRoutingQueryResponse>>
    {
        public Guid FormVersionId { get; set; }
    }
}