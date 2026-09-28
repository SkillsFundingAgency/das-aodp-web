using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Qualifications;

namespace SFA.DAS.AODP.Application.Services;

public interface IQualificationTimelineHistoryBuilder
{
    List<QualificationDiscussionHistory> BuildTimelineEntries(List<GetQualificationDetailsQueryResponse> versions);

    List<KeyFieldChange> GetKeyFieldChanges(GetQualificationDetailsQueryResponse latestVersion, GetQualificationDetailsQueryResponse previousVersion);
}
