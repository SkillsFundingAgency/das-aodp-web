using SFA.DAS.AODP.Domain.Interfaces;

public class GetFundingOffersApiRequest : IGetApiRequest
{
    public Guid ApplicationId { get; set; }

    public string GetUrl => $"api/funding-offers";

}