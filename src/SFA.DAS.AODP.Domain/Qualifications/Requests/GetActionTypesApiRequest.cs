using SFA.DAS.AODP.Domain.Interfaces;

public class GetActionTypesApiRequest : IGetApiRequest
{
    public string BaseUrl = "api/qualifications/GetActionTypes";

    public string GetUrl => "api/qualifications/GetActionTypes";
}
 