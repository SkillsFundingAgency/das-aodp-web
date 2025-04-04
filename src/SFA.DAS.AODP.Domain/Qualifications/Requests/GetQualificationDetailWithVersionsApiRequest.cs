using SFA.DAS.AODP.Domain.Interfaces;

public class GetQualificationDetailWithVersionsApiRequest : IGetApiRequest
{
    public  string QualificationReference { get; set; }

    public GetQualificationDetailWithVersionsApiRequest(string qualificationReference)
    {
        QualificationReference = qualificationReference;
    }

    public string GetUrl => $"api/qualifications/{QualificationReference}/detailwithversions";
}
