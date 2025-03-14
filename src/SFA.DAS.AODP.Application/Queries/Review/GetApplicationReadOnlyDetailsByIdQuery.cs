using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Review;

public class GetApplicationReadOnlyDetailsByIdQuery : IRequest<BaseMediatrResponse<GetApplicationReadOnlyDetailsByIdQueryResponse>>
{
    public Guid ApplicationReviewId { get; set; }

    public GetApplicationReadOnlyDetailsByIdQuery(Guid applicationReviewId)
    {
        ApplicationReviewId = applicationReviewId;
    }
}
