using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Authentication;

namespace SFA.DAS.AODP.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthenticationController(IAuthorizationService authorizationService)
        {
            this._authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index()
        {
            var isReview = _authorizationService.AuthorizeAsync(User, PolicyConstants.IsReviewUser).Result;
            if (isReview.Succeeded)
                return Redirect("/review");

            var IsApplyImport = _authorizationService.AuthorizeAsync(User, PolicyConstants.IsApplyUser).Result;
            if (IsApplyImport.Succeeded)
                return Redirect("/apply");

            var IsAdminFormUser = _authorizationService.AuthorizeAsync(User, PolicyConstants.IsAdminFormsUser).Result;
            if (IsAdminFormUser.Succeeded)
                return Redirect("/admin/forms");

            var IsAdminImportUser = _authorizationService.AuthorizeAsync(User, PolicyConstants.IsAdminImportUser).Result;
            if (IsAdminImportUser.Succeeded)
                return Redirect("/admin/import");

            return View("Error");
        }

        public async Task<IActionResult> AutoLogin()
        {
            await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/Authentication/" });
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SignOut()
        {

            var idToken = await HttpContext.GetTokenAsync("id_token");
            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token", idToken);
            authenticationProperties.Parameters.Add("post_logout_redirect_uri", "/signout");
            return SignOut(authenticationProperties,
                CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);

        }

        public async Task<IActionResult> SignIn()
        {
            // When using Stub Auth dont redirect to the login challenge, as it doesnt exist. User Auth already configured so no need.            
            if (!User.Identity.IsAuthenticated)
            {
                return Forbid();
                //await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/Authentication/" });
            }
            return RedirectToAction(nameof(Index));

        }
    }
}
