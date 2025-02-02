using MediatR;
using SFA.DAS.AODP.Application.Queries.Qualifications;

namespace SFA.DAS.AODP.Application.Queries.Test
{
    public class GetNewQualificationsQuery : IRequest<GetNewQualificationsQueryResponse>
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }
}
