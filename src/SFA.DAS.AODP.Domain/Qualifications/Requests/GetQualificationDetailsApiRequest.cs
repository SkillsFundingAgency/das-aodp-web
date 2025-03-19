using SFA.DAS.AODP.Domain.Interfaces;
using System.Net.NetworkInformation;

public class GetQualificationDetailsApiRequest : IGetApiRequest
{
    private readonly string _qualificationReference;
    private readonly string _status;

    public GetQualificationDetailsApiRequest(string qualificationReference,string status)
    {
        _qualificationReference = qualificationReference;
        _status = status;   
    }

    public string GetUrl => $"api/qualifications/{_qualificationReference}";
}
