using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Controllers.Application
{
    public class ApplicationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
