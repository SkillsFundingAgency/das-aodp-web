using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationsForReviewApiRequest : IPostApiRequest
{
    public string PostUrl => $"/api/application-reviews";
    public object Data { get; set; }
}
