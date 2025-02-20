using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationByIdRequest : IGetApiRequest
{
    public Guid ApplicationId { get; set; }

    public string GetUrl => $"/api/applications/{ApplicationId}";

}