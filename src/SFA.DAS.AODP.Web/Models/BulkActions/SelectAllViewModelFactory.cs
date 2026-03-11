using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Web.Models.BulkActions
{
    [ExcludeFromCodeCoverage]
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
    }
}
