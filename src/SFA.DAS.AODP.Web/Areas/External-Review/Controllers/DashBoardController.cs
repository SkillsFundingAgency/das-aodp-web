using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Areas.ExternalReview.Controllers
{
    [Area("External-Review")]
    [Authorize(Policy ="IsDFEUser")]
    public class DashBoardController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public DashBoardController(IAuthorizationService authorizationService)
        {
            this._authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
