using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Controllers;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    public class ApplicationsReviewController : ControllerBase
    {

        public ApplicationsReviewController(ILogger<ApplicationsReviewController> logger, IMediator mediator) : base(mediator, logger)
        { }

        [Route("review/applications-review")]
        public async Task<IActionResult> Index(ApplicationReviewFilters filters)
        {

            try
            {
                var response = await Send(new GetApplicationsForReviewQuery()
                {
                    ReviewUser = AODP.Models.Users.UserType.Qfau.ToString(),
                    ApplicationStatuses = filters.Status,
                    ApplicationsWithNewMessages = filters.Status?.Contains(ApplicationStatus.NewMessage) == true,
                    ApplicationSearch = filters.ApplicationSearch,
                    AwardingOrganisationSearch = filters.AwardingOrganisationSearch,
                    Limit = filters.PageSize,
                    Offset = filters.PageSize * (filters.PageNumber - 1)

                });
                return View(new ApplicationsReviewListViewModel());

            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }
    }

    public class ApplicationReviewFilters
    {
        public string? ApplicationSearch { get; set; }
        public string? AwardingOrganisationSearch { get; set; }
        public List<ApplicationStatus> Status { get; set; }
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
    }
}
