using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models.Import;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ImportController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost()]
        [Route("/admin/import/confirm")]
        public async Task<IActionResult> SelectImport(ImportViewModel model)
        {

            return View("Confirm", model);
        }

        [HttpPost()]
        [Route("/admin/import/check")]
        public async Task<IActionResult> ConfirmImport(ImportViewModel model)
        {

            return View("Progress", model);
        }

        [HttpPost()]
        [Route("/admin/import/complete")]
        public async Task<IActionResult> CheckProgress(ImportViewModel model)
        {

            return View("Complete", model);
        }

    }
}
