using SFA.DAS.AODP.Domain.Interfaces;

public class SaveQualificationsFundingOffersOutcomeApiRequest : IPutApiRequest
{
    public Guid QualificationVersionId { get; set; }
    public string PutUrl => $"api/qualifications/{QualificationVersionId}/save-qualification-funding-offers-outcome";
    public object Data { get; set; }
}
