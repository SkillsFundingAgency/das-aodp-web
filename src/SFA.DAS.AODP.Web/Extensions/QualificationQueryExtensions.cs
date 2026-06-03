using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.Extensions
{
    public static class QualificationQueryExtensions
    {
        public static GetNewQualificationsQuery ToGetNewQualificationsQuery(this QualificationQuery q)
        {
            var query = new GetNewQualificationsQuery
            {
                Take = q.RecordsPerPage,
                Skip = q.PageNumber > 0
                    ? q.RecordsPerPage * (q.PageNumber - 1)
                    : 0
            };

            if (!string.IsNullOrWhiteSpace(q.Name))
                query.Name = q.Name;

            if (!string.IsNullOrWhiteSpace(q.Organisation))
                query.Organisation = q.Organisation;

            if (!string.IsNullOrWhiteSpace(q.Qan))
                query.QAN = q.Qan;

            if (q.ProcessStatusIds?.Count > 0)
            {
                query.ProcessStatusFilter = new Domain.Models.ProcessStatusFilter
                {
                    ProcessStatusIds = q.ProcessStatusIds
                };
            }

            if (q.AgeGroups?.Count > 0)
            {
                query.AgeGroups = q.AgeGroups;
            }

            return query;
        }

        public static NewQualificationFilterViewModel ToQualificationFilterViewModel(this QualificationQuery q)
        {
            return new NewQualificationFilterViewModel
            {
                Organisation = q.Organisation ?? string.Empty,
                QualificationName = q.Name ?? string.Empty,
                QAN = q.Qan ?? string.Empty,
                ProcessStatusIds = q.ProcessStatusIds ?? new(),
                AgeGroups = q.AgeGroups ?? new()
            };
        }

        public static GetChangedQualificationsQuery ToGetChangedQualificationsQuery(this QualificationQuery q)
        {
            return new GetChangedQualificationsQuery
            {
                Take = q.RecordsPerPage,
                Skip = q.PageNumber > 0
                    ? q.RecordsPerPage * (q.PageNumber - 1)
                    : 0,

                Name = string.IsNullOrWhiteSpace(q.Name)
                    ? null
                    : q.Name,

                Organisation = string.IsNullOrWhiteSpace(q.Organisation)
                    ? null
                    : q.Organisation,

                QAN = string.IsNullOrWhiteSpace(q.Qan)
                    ? null
                    : q.Qan,

                ProcessStatusIds = q.ProcessStatusIds ?? new(),

                AgeGroups = q.AgeGroups ?? new()
            };
        }

        public static object ToRouteValues(this QualificationQuery q, int? pageNumberOverride = null)
            => new
            {
                pageNumber = pageNumberOverride ?? q.PageNumber,
                recordsPerPage = q.RecordsPerPage,
                name = q.Name,
                organisation = q.Organisation,
                qan = q.Qan,
                processStatusIds = q.ProcessStatusIds,
                ageGroups = q.AgeGroups,
            };
    }
}
