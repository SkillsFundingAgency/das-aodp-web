using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Areas.Review.Models.Home;
using SFA.DAS.AODP.Web.Helpers.User;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
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

            return Redirect("/review/application-reviews");
        }

        [HttpPost]
        public IActionResult Index(ReviewHomeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.UserRoles = _userHelperService.GetUserRoles();
                return View(model);
            }

            if (model.SelectedOption == ReviewHomeViewModel.Options.ApplicationsReview)
            {
                return RedirectToAction("Index", "ApplicationsReview");
            }

            else if (model.SelectedOption == ReviewHomeViewModel.Options.NewQualifications)
            {
                return RedirectToAction("Index", "New");
            }

            else if (model.SelectedOption == ReviewHomeViewModel.Options.ChangedQualifications)
            {
                return RedirectToAction("Index", "Changed");
            }

            else if (model.SelectedOption == ReviewHomeViewModel.Options.ImportData)
            {
                return RedirectToAction("Index", "Import");
            }

            else if (model.SelectedOption == ReviewHomeViewModel.Options.FormsManagement)
            {
                return RedirectToAction("Index", "Forms");
            }

            else if (model.SelectedOption == ReviewHomeViewModel.Options.OutputFile)
            {
                return RedirectToAction("Index", "OutputFile");
            }

            else return NotFound();

        }
    }
}
