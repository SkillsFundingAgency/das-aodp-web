using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationFormStatusByApplicationIdApiRequest : IGetApiRequest
{
    public Guid ApplicationId { get; set; }
    public Guid FormVersionId { get; set; }

    public string GetUrl => $"api/applications/{ApplicationId}/forms/{FormVersionId}";

}