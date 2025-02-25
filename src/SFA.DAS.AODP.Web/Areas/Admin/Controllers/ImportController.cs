using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ImportController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
