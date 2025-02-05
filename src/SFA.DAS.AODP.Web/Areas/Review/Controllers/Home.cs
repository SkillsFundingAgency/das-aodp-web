using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    public class Home : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
