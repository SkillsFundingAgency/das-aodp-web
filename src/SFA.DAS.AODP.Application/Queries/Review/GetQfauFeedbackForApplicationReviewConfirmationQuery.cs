using MediatR;
using SFA.DAS.AODP.Application;

public class GetQfauFeedbackForApplicationReviewConfirmationQuery : IRequest<BaseMediatrResponse<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>>
{
    public Guid ApplicationReviewId { get; set; }

    public GetQfauFeedbackForApplicationReviewConfirmationQuery(Guid applicationReviewId)
    {
        ApplicationReviewId = applicationReviewId;
    }
}
