using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models;
using System.Diagnostics;

namespace SFA.DAS.AODP.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUserHelperService _userHelperService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IUserHelperService userHelperService)
        {
            _logger = logger;
            _userHelperService = userHelperService;
        }

        public IActionResult Index()
        {
            var userType = _userHelperService.GetUserType();

            if (userType == AODP.Models.Users.UserType.AwardingOrganisation)
            {
                return Redirect("/apply/applications");
            }

            return Redirect("/review");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
