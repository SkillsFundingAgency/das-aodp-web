using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationDetailsQuery : IRequest<BaseMediatrResponse<GetChangedQualificationDetailsQueryResponse>>
    {
        public string QualificationReference { get; set; }
    }
}
