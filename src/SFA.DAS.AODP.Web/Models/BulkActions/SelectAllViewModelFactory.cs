using SFA.DAS.AODP.Models.Application;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{

    public static class SelectAllViewModelFactory
    {
        public static SelectAllCheckboxesViewModel ForQualifications(
            int currentPage,
            int recordsPerPage,
            string controllerName,
            string? name,
            string? organisation,
            string? qan,
            IEnumerable<Guid>? processStatusIds)
            {
                var routeValues = new Dictionary<string, object?>();

                if (!string.IsNullOrWhiteSpace(name))
                    routeValues["name"] = name;

                if (!string.IsNullOrWhiteSpace(organisation))
                    routeValues["organisation"] = organisation;

                if (!string.IsNullOrWhiteSpace(qan))
                    routeValues["qan"] = qan;

                if (processStatusIds != null && processStatusIds.Any())
                    routeValues["processStatusIds"] = processStatusIds;

                return new SelectAllCheckboxesViewModel
                {
                    CurrentPage = currentPage,
                    RecordsPerPage = recordsPerPage,
                    Controller = controllerName,
                    Action = "Index",
                    Area = "Review",
                    RouteValues = routeValues
                };
            }

        public static SelectAllCheckboxesViewModel ForApplications(
            int currentPage,
            int recordsPerPage,
            string controllerName,
            string? applicationSearch,
            string? awardingOrganisationSearch,
            string? reviewerSelection,
            IEnumerable<ApplicationStatus>? status
            )
        {
            var routeValues = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(applicationSearch))
                routeValues["applicationSearch"] = applicationSearch;

            if (!string.IsNullOrWhiteSpace(awardingOrganisationSearch))
                routeValues["awardingOrganisationSearch"] = awardingOrganisationSearch;

            if (!string.IsNullOrWhiteSpace(reviewerSelection))
                routeValues["reviewerSelection"] = reviewerSelection;

            if (status != null && status.Any())
                routeValues["status"] = status;

            return new SelectAllCheckboxesViewModel
            {
                CurrentPage = currentPage,
                RecordsPerPage = recordsPerPage,
                Controller = controllerName,
                Action = "Index",
                Area = "Review",
                RouteValues = routeValues
            };
        }
    }
}
