using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Controllers
{
    public class RedirectHandler: Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public RedirectHandler(IAuthorizationService authorizationService)
        {
            this._authorizationService = authorizationService;
        }

        public async Task<IActionResult> Index()
        {
            var isQFAU = _authorizationService.AuthorizeAsync(User, "IsDFEUser").Result;
            if (isQFAU.Succeeded)
                return RedirectToAction("Index", "DashBoard", new { area = "Review" });

            var isAO =   _authorizationService.AuthorizeAsync(User, "IsFundingUser").Result;
            if(isAO.Succeeded)
                return RedirectToAction("Index","DashBoard",new {area="Funding"});

         

            var isIFATE= _authorizationService.AuthorizeAsync(User, "IsExternalReviewUser").Result;
            if (isIFATE.Succeeded)
                return RedirectToAction("Index", "DashBoard", new { area= "External-Review" });

            return View("Error");
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
                await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/RedirectHandler" });
            }

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
