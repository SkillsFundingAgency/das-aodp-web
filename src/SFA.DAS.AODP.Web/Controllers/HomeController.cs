using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace SFA.DAS.AODP.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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

        [Authorize]
        [HttpGet]
        [Route("signin", Name = "provider-signin")]
        public async Task SignIn()
        {
            await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri="/dashboard"});
        }
    }
}
