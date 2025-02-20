using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
