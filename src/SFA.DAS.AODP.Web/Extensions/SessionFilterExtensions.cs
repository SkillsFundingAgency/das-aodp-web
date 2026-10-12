using SFA.DAS.AODP.Web.Models.Session;
using SFA.DAS.AODP.Application.Queries.Qualifications;

namespace SFA.DAS.AODP.Web.Extensions
{
    public static class SessionFilterExtensions
    {
        public static GetNewQualificationsQuery ToNewQualificationsQuery(this QualificationFilterSessionModel s)
        {
            return new GetNewQualificationsQuery
            {
                Take = s.RecordsPerPage,
                Skip = s.PageNumber > 0
                    ? s.RecordsPerPage * (s.PageNumber - 1)
                    : 0,

                Name = string.IsNullOrWhiteSpace(s.QualificationName) ? null : s.QualificationName,
                Organisation = string.IsNullOrWhiteSpace(s.Organisation) ? null : s.Organisation,
                QAN = string.IsNullOrWhiteSpace(s.QAN) ? null : s.QAN,

                ProcessStatusFilter = s.ProcessStatusIds?.Count > 0
                    ? new Domain.Models.ProcessStatusFilter { ProcessStatusIds = s.ProcessStatusIds }
                    : null,

                AgeGroups = s.AgeGroups ?? new()
            };
        }

        public static GetChangedQualificationsQuery ToChangedQualificationsQuery(this QualificationFilterSessionModel s)
        {
            return new GetChangedQualificationsQuery
            {
                Take = s.RecordsPerPage,
                Skip = s.PageNumber > 0
                    ? s.RecordsPerPage * (s.PageNumber - 1)
                    : 0,

                Name = string.IsNullOrWhiteSpace(s.QualificationName) ? null : s.QualificationName,
                Organisation = string.IsNullOrWhiteSpace(s.Organisation) ? null : s.Organisation,
                QAN = string.IsNullOrWhiteSpace(s.QAN) ? null : s.QAN,

                ProcessStatusIds = s.ProcessStatusIds ?? new(),
                AgeGroups = s.AgeGroups ?? new()
            };
        }
    }
}
