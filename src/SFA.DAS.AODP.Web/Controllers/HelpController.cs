using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Authentication;

namespace SFA.DAS.AODP.Web.Controllers
{
    public class HelpController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        //[Route("cookies", Name = RouteConstants.Cookies)]
        public IActionResult CookiesPolicy()
        {
            return View();
        }
    }
}
