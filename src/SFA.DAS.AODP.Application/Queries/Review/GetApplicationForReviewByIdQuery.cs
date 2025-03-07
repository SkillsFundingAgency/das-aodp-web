using MediatR;
using SFA.DAS.AODP.Application;

public class GetApplicationForReviewByIdQuery : IRequest<BaseMediatrResponse<GetApplicationForReviewByIdQueryResponse>>
{
    public Guid ApplicationReviewId { get; set; }

    public GetApplicationForReviewByIdQuery(Guid applicationReviewId)
    {
        ApplicationReviewId = applicationReviewId;
    }
}
