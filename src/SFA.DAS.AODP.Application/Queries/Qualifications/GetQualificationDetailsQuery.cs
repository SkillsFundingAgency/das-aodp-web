using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationDetailsQuery : IRequest<GetQualificationDetailsQueryResponse>
    {
        public int Id { get; set; }
    }
}
