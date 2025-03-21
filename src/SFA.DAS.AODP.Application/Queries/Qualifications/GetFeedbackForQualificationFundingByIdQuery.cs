using MediatR;
using SFA.DAS.AODP.Application;

public class GetFeedbackForQualificationFundingByIdQuery : IRequest<BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>>
{
    public Guid QualificationVersionId { get; set; }

    public GetFeedbackForQualificationFundingByIdQuery(Guid qualificationVersionId)
    {
        QualificationVersionId = qualificationVersionId;
    }
}
