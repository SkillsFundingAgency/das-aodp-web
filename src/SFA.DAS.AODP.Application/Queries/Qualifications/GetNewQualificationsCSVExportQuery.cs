using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetNewQualificationsCsvExportQuery: IRequest<BaseMediatrResponse<GetQualificationsExportResponse>>
    {
    }
}
