using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationDetailsQuery : IRequest<GetQualificationDetailsQueryResponse>
    {
        public string QualificationReference { get; set; }
    }
}
