using SFA.DAS.AODP.Domain.Interfaces;

public class GetChangedQualificationDetailsApiRequest : IGetApiRequest
{
    private readonly string _qualificationReference;

    public GetChangedQualificationDetailsApiRequest(string qualificationReference)
    {
        _qualificationReference = qualificationReference;
    }

    public string GetUrl => $"api/qualifications/{_qualificationReference}";
}
 