using SFA.DAS.AODP.Domain.Interfaces;

public class GetQfauFeedbackForApplicationReviewConfirmationApiRequest : IGetApiRequest
{
    public Guid ApplicationReviewId { get; set; }

    public GetQfauFeedbackForApplicationReviewConfirmationApiRequest(Guid applicationReviewId)
    {
        ApplicationReviewId = applicationReviewId;
    }

    public string GetUrl => $"api/application-reviews/{ApplicationReviewId}/qfau-feedback-review";

}