using SFA.DAS.AODP.Domain.Interfaces;

public class SaveQualificationsFundingOffersDetailsApiRequest : IPutApiRequest
{
    public Guid QualificationVersionId { get; set; }

    public string PutUrl => $"api/qualifications/{QualificationVersionId}/save-qualification-funding-offers-details";
    public object Data { get; set; }
}
