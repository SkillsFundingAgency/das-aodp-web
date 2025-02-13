using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationFormByIdApiRequest : IGetApiRequest
{
    public Guid FormVersionId { get; set; }

    public string GetUrl => $"api/applications/forms/{FormVersionId}";
}
