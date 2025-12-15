using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Areas.Review.Models.Home;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUserHelperService _userHelperService;

        public HomeController(IUserHelperService userHelperService)
        {
            _userHelperService = userHelperService;
        }

        public IActionResult Index()
        {
            var userType = _userHelperService.GetUserType();

            if (userType == AODP.Models.Users.UserType.Qfau)
            {
                return View(new ReviewHomeViewModel()
                {
                    UserRoles = _userHelperService.GetUserRoles()
                });
            }

            return RedirectToAction("Index", "ApplicationsReview");
        }
    }
}
