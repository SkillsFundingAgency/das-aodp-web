using MediatR;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Routes
{
    public class GetAvailableQuestionsForRoutingQuery : IRequest<BaseMediatrResponse<GetAvailableQuestionsForRoutingQueryResponse>>
    {
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid PageId { get; set; }
    }
}