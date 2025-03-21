using SFA.DAS.AODP.Domain.Interfaces;

public class SaveQualificationsFundingOffersApiRequest : IPutApiRequest
{
    public Guid QualificationVersionId { get; set; }
    public string PutUrl => $"api/qualifications/{QualificationVersionId}/save-qualification-funding-offers";
    public object Data { get; set; }
}
