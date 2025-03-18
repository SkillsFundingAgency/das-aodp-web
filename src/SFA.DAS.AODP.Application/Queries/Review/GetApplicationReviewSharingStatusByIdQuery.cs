using MediatR;
using SFA.DAS.AODP.Application;

public class GetApplicationReviewSharingStatusByIdQuery : IRequest<BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>>
{
    public Guid ApplicationReviewId { get; set; }

    public GetApplicationReviewSharingStatusByIdQuery(Guid applicationReviewId)
    {
        ApplicationReviewId = applicationReviewId;
    }
}
