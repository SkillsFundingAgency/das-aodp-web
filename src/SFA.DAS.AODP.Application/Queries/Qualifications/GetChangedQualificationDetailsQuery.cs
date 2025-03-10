using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationDetailsQuery : IRequest<BaseMediatrResponse<GetChangedQualificationDetailsResponse>>
    {
        public string QualificationReference { get; set; }
        public string Status { get; set; }
    }
}
