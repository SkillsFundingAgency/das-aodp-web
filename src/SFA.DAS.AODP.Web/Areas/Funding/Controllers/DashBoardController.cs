using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Areas.Funding.Controllers
{
    [Area("Funding")]
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
        [Authorize(Policy= "IsAOUser")]
        public IActionResult AO()
        {
            return View("AO");
        }
        [Authorize(Policy = "IsQFAUUser")]
        public IActionResult QFAU()
        {
            return View("QFAU");
        }

        [Authorize(Policy="IsIFATEUser")]
        public IActionResult IFATE()
        {
            return View("IFATE");
        }

        [Authorize(Policy = "IsOFQUALUser")]
        public IActionResult OFQUAL()
        {

            return View("OFQUAL");
        }
    }
}
