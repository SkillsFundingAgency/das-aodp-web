using MediatR;
using SFA.DAS.AODP.Application;

public class GetFeedbackForQualificationFundingByIdQuery : IRequest<BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>>
{
    public Guid QualificationId { get; set; }

    public GetFeedbackForQualificationFundingByIdQuery(Guid qualificationId)
    {
        QualificationId = qualificationId;
    }
}
