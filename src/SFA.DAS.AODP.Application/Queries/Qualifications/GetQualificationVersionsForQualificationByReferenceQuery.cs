using MediatR;
using SFA.DAS.AODP.Application;

public class GetQualificationVersionsForQualificationByReferenceQuery : IRequest<BaseMediatrResponse<GetQualificationVersionsForQualificationByReferenceQueryResponse>>
{
    public string QualificationReference { get; set; }

    public GetQualificationVersionsForQualificationByReferenceQuery(string qualificationReference)
    {
        QualificationReference = qualificationReference;
    }
}
