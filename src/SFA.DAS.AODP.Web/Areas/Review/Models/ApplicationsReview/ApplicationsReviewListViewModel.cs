using Azure;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Constants;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class ApplicationsReviewListViewModel
    {
        public List<Application> Applications { get; set; } = new();
        public int? TotalItems { get; set; }

        public int Page { get; set; } = 1;
        public int ItemsPerPage { get; set; } = 10;

        public string? ApplicationSearch { get; set; }
        public string? AwardingOrganisationSearch { get; set; }
        public List<SelectListItem> ReviewerOptions { get; set; } = new();
        public string? ReviewerSelection { get; set; }
        public bool UnassignedOnly =>
            string.Equals(
                ReviewerSelection?.Trim(), 
                ReviewerDropdown.Filter.UnassignedValue, 
                StringComparison.Ordinal);
        public string? ReviewerSearch =>
            UnassignedOnly 
            ? null
            : string.IsNullOrWhiteSpace(ReviewerSelection) ? null : ReviewerSelection.Trim(); 
        public List<ApplicationStatus> Status { get; set; }
        public string UserType { get; set; }
        public string FindRegulatedQualificationUrl { get; set; } = string.Empty;

        public class Application
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime LastUpdated { get; set; }
            public string? Owner { get; set; }
            public int Reference { get; set; }
            public string? Qan { get; set; }
            public ApplicationStatus Status { get; set; }
            public bool NewMessage { get; set; }
            public string? AwardingOrganisation { get; set; }
            public Guid ApplicationReviewId { get; set; }
            public List<string> ReviewersSummary { get; set; }
        }

        public void MapApplications(GetApplicationsForReviewQueryResponse response)
        {
            TotalItems = response.TotalRecordsCount;
            foreach (var application in response.Applications)
            {
                var reviewers = new[] { application.Reviewer1, application.Reviewer2 }
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .ToList();

                Applications.Add(new()
                {
                    Id = application.Id,
                    Name = application.Name,
                    LastUpdated = application.LastUpdated,
                    Owner = application.Owner,
                    Reference = application.Reference,
                    Qan = application.Qan,
                    Status = application.Status,
                    AwardingOrganisation = application.AwardingOrganisation,
                    NewMessage = application.NewMessage,
                    ApplicationReviewId = application.ApplicationReviewId,
                    ReviewersSummary = GetReviewersForSearchResults(application)

                });
            }

            ReviewerOptions = GetReviewersForSearch(response.AvailableReviewers).ToList();       
        }
        private IEnumerable<SelectListItem> GetReviewersForSearch(
            IEnumerable<UserOption> availableReviewers,
            string? selectedReviewer = null)
        {
            selectedReviewer = selectedReviewer?.Trim();

            var items = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = ReviewerDropdown.Filter.PlaceholderValue,
                    Text = ReviewerDropdown.Filter.PlaceholderText,
                    Selected = string.IsNullOrWhiteSpace(selectedReviewer),
                    Disabled = true
                },
                new SelectListItem
                {
                    Value = ReviewerDropdown.Filter.UnassignedValue,
                    Text = ReviewerDropdown.Filter.UnassignedText,
                    Selected = selectedReviewer == ReviewerDropdown.Filter.UnassignedValue
                }
            };

            items.AddRange(
            availableReviewers
                .OrderBy(r => r.LastName)
                .ThenBy(r => r.FirstName)
                .Select(r =>
                {
                    var name = $"{r.FirstName} {r.LastName}".Trim();
                    return new SelectListItem
                    {
                        Value = name,
                        Text = name,
                        Selected = selectedReviewer == name
                    };
                })
            );

            return items;
        }

        private List<string> GetReviewersForSearchResults(GetApplicationsForReviewQueryResponse.Application application)
        {
            var reviewers = new[]
            {
                application.Reviewer1?.Trim(),
                application.Reviewer2?.Trim()
            }
            .Where(r => !string.IsNullOrEmpty(r))
            .ToList();

            if (reviewers.Count == 0)
            {
                reviewers.Add(ReviewerDisplayText.NoneAssigned);
            }

            return reviewers;
        }
    }
}
