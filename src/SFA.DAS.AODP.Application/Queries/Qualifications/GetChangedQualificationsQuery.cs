using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationsQuery : IRequest<BaseMediatrResponse<GetChangedQualificationsQueryResponse>>
    {
    }
}
