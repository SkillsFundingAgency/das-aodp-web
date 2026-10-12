using Azure;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Models.Applications;
using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Models.GdsComponents;
using SFA.DAS.AODP.Web.Models.Qualifications;
using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Patterns;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class ApplicationsReviewListViewModel :ApplicationsBulkActionPageViewModel
    {
        public List<Application> Applications { get; set; } = new();

        // Business‑logic pagination:
        // Calculates current page, total pages, start/end record numbers.
        public PaginationViewModel PaginationViewModel { get; set; }

        // UI pagination model for the GOV.UK component:
        // Builds pagination URLs for the GOV.UK component.
        public PaginationModel GdsPagination { get; set; } = new PaginationModel();

        [AllowedCharacters(TextCharacterProfile.Title)]
        public string? ApplicationSearch { get; set; }

        [AllowedCharacters(TextCharacterProfile.Title)]
        public string? AwardingOrganisationSearch { get; set; }
        public string? ReviewerSelection { get; set; }
        public bool UnassignedOnly =>
            string.Equals(
                ReviewerSelection?.Trim(), 
                ReviewerDropdown.UnassignedValue, 
                StringComparison.Ordinal);
        public string? ReviewerSearch =>
            UnassignedOnly 
            ? null
            : string.IsNullOrWhiteSpace(ReviewerSelection) ? null : ReviewerSelection.Trim(); 
        public List<ApplicationStatus> Status { get; set; }
        public string UserType { get; set; }
        public string FindRegulatedQualificationUrl { get; set; } = string.Empty;

        public string AvailableReviewersJson { get; set; } = string.Empty;

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
            public List<string> ReviewersSummary { get; set; } = new();
            public string SubmittedDate { get; set; } = string.Empty; 

        }

        public static ApplicationsReviewListViewModel Map(
            GetApplicationsForReviewQueryResponse response,
            ApplicationsReviewQuery query)
        {
            var vm = new ApplicationsReviewListViewModel();

            vm.MapApplications(response);

            vm.ApplicationSearch = query.ApplicationSearch;
            vm.AwardingOrganisationSearch = query.AwardingOrganisationSearch;
            vm.ReviewerSelection = query.ReviewerSelection;
            vm.Status = query.Status ?? new List<ApplicationStatus>();

            vm.PaginationViewModel = new PaginationViewModel(
                response.TotalRecordsCount,
                (query.PageNumber - 1) * query.RecordsPerPage,
                query.RecordsPerPage);

            vm.GdsPagination = new PaginationModel()
            {
                CurrentPage = vm.PaginationViewModel.CurrentPage,
                MaxPageNumber = vm.PaginationViewModel.TotalPages,
                ActionName = "Index",
                ControllerName = "ApplicationsReview",
                Area = "Review"
            };

            return vm;
        }

        public void MapApplications(GetApplicationsForReviewQueryResponse response)
        {
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
                    ReviewersSummary = GetReviewersForSearchResults(application),
                    SubmittedDate = application.SubmittedAt.HasValue 
                        ? application.SubmittedAt.Value.ToDateDisplayFormat()
                        : string.Empty

                });
            }

            ReviewerOptions = GetReviewersForSearchFilter(response.AvailableReviewers).ToList();       
        }
        private IEnumerable<SelectListItem> GetReviewersForSearchFilter(
            IEnumerable<UserOption> availableReviewers,
            string? selectedReviewer = null)
        {
            selectedReviewer = selectedReviewer?.Trim();

            var items = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = ReviewerDropdown.PlaceholderValue,
                    Text = ReviewerDropdown.PlaceholderText,
                    Selected = string.IsNullOrWhiteSpace(selectedReviewer),
                    Disabled = true
                },
                new SelectListItem
                {
                    Value = ReviewerDropdown.UnassignedValue,
                    Text = ReviewerDropdown.UnassignedText,
                    Selected = selectedReviewer == ReviewerDropdown.UnassignedValue
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
