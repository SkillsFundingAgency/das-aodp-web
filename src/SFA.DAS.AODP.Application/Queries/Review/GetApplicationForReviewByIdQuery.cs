using MediatR;
namespace SFA.DAS.AODP.Application.Queries.Review;

public class GetApplicationForReviewByIdQuery : IRequest<BaseMediatrResponse<GetApplicationForReviewByIdQueryResponse>>
{
    public Guid ApplicationReviewId { get; set; }

    public GetApplicationForReviewByIdQuery(Guid applicationReviewId)
    {
        ApplicationReviewId = applicationReviewId;
    }
}
