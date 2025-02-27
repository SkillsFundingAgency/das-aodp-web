using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    public class ApplicationsReviewController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
