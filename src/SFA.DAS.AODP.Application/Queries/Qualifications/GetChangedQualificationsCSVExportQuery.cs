using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetChangedQualificationsCsvExportQuery: IRequest<BaseMediatrResponse<GetQualificationsExportResponse>>
    {
    }
}
