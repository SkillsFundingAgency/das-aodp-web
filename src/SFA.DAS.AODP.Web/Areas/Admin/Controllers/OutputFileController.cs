using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Authentication;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PolicyConstants.IsAdminImportUser)]
    public class OutputFileController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
