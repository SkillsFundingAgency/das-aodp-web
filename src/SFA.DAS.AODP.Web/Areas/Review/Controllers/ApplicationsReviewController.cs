using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    public class ApplicationsReviewController : Controller
    {
        [Route("review/applications-review")]
        public async Task<IActionResult> Index()
        {
            return View(new ApplicationsReviewListViewModel());
        }
    }
}
