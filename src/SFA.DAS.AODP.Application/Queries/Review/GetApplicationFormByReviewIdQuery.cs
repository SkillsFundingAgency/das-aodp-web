using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Review;

public class GetApplicationFormByReviewIdQuery : IRequest<BaseMediatrResponse<GetApplicationFormByReviewIdQueryResponse>>
{
    public Guid ApplicationReviewId { get; set; }

    public GetApplicationFormByReviewIdQuery(Guid applicationReviewId)
    {
        ApplicationReviewId = applicationReviewId;
    }
}
