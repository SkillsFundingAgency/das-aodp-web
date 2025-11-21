using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Authentication;

namespace SFA.DAS.AODP.Web.Areas.Import.Controllers;

[Area("Import")]
[Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
[Route("import")]
public class ImportController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
