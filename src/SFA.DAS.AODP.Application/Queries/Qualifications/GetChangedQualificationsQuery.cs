using MediatR;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationsQuery : IRequest<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>
    {
        public string? Name { get; set; }
        public string? Organisation { get; set; }
        public string? QAN { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public List<Guid>? ProcessStatusIds { get; set; } = new();
        public List<AgeGroup> AgeGroups { get; set; } = new();
    }
}

