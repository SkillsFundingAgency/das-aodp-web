using SFA.DAS.AODP.Domain.Interfaces;

public class GetQualificationDetailsApiRequest : IGetApiRequest
{
    private readonly string _qualificationReference;

    public GetQualificationDetailsApiRequest(string qualificationReference)
    {
        _qualificationReference = qualificationReference;
    }

    public string GetUrl => $"api/qualifications/new/{_qualificationReference}";
}
