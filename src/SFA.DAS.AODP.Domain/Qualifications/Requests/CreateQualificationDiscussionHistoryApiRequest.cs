using SFA.DAS.AODP.Domain.Interfaces;

public class CreateQualificationDiscussionHistoryApiRequest : IPutApiRequest
{
    public Guid QualificationVersionId { get; set; }
    public string PutUrl => $"api/qualifications/{QualificationVersionId}/Create-QualificationDiscussionHistory";
    public object Data { get; set; }
}
