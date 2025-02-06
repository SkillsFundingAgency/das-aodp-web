using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationFormsApiRequest : IGetApiRequest
{
    public string GetUrl => $"/api/applications/forms";
}
