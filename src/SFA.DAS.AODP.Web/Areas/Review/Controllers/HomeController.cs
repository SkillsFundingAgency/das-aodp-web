using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;


namespace SFA.DAS.AODP.Web.Area.Review.Controllers
{
    [Area("Review")]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthorizationService _authorizationService;

        public HomeController(ILogger<HomeController> logger, IAuthorizationService authorizationService)
        {
            _logger = logger;
            this._authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpGet]
        [Route("signout", Name = "provider-signout")]
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

        [HttpGet]
        public async Task SignIn()
        {
            // When using Stub Auth dont redirect to the login challenge, as it doesnt exist. User Auth already configured so no need.            
            if (!User.Identity.IsAuthenticated)
            {
                await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/Review/DashBoard" });
            }

        }
    }
}
