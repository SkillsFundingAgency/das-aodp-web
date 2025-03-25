using SFA.DAS.AODP.Domain.Interfaces;

public class UpdateApplicationReviewSharingApiRequest(Guid applicationReviewId) : IPutApiRequest
{
    public string PutUrl => $"api/application-reviews/{applicationReviewId}/share";

    public object Data { get; set; }
}
