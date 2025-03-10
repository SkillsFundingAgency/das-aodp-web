using SFA.DAS.AODP.Domain.Interfaces;

public class GetChangedQualificationDetailsApiRequest : IGetApiRequest
{
    private readonly string _qualificationReference;
    private readonly string _status;

    public GetChangedQualificationDetailsApiRequest(string qualificationReference,string status)
    {
        _qualificationReference = qualificationReference;
        _status = status;
    }

    public string GetUrl => $"api/qualifications/{_status}/{_qualificationReference}";
}
 