using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    [Authorize(Policy="IsDFEUser")]
    public class DashBoardController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public DashBoardController(IAuthorizationService auth5orizationService)
        {
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}