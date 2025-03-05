using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Helpers.User;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    public class ApplicationsReviewController : ControllerBase
    {

        private readonly IUserHelperService _userHelperService;
        public ApplicationsReviewController(ILogger<ApplicationsReviewController> logger, IMediator mediator, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _userHelperService = userHelperService;
        }

        [Route("review/applications-review")]
        public async Task<IActionResult> Index(ApplicationsReviewListViewModel model)
        {

            try
            {
                string userType = _userHelperService.GetUserType().ToString();
                var response = await Send(new GetApplicationsForReviewQuery()
                {
                    ReviewUser = userType,
                    ApplicationStatuses = model.Status?.Select(s => s.ToString()).ToList(),
                    ApplicationsWithNewMessages = model.Status?.Contains(ApplicationStatus.NewMessage) == true,
                    ApplicationSearch = model.ApplicationSearch,
                    AwardingOrganisationSearch = model.AwardingOrganisationSearch,
                    Limit = model.ItemsPerPage,
                    Offset = model.ItemsPerPage * (model.Page - 1)

                });

                model.MapApplications(response);
                model.UserType = userType;
                return View(model);

            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [HttpPost]
        [Route("review/applications-review")]
        public async Task<IActionResult> Search(ApplicationsReviewListViewModel model)
        {
            return RedirectToAction(nameof(Index), new ApplicationsReviewListViewModel()
            {
                Page = 1,
                ItemsPerPage = model.ItemsPerPage,
                ApplicationSearch = model.ApplicationSearch,
                AwardingOrganisationSearch = model.AwardingOrganisationSearch,
                Status = model.Status
            });
        }
    }
}
