using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FormsBuilderController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
