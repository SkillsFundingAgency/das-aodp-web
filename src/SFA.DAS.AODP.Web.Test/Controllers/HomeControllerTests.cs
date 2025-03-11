//using System.Diagnostics;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using SFA.DAS.AODP.Web.Models;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.Extensions.Logging;

//namespace SFA.DAS.AODP.Web.Controllers
//{
//    public class HomeControllerTests : Controller
//    {
//        private readonly ILogger<HomeControllerTests> _logger;

//        public HomeControllerTests(ILogger<HomeControllerTests> logger)
//        {
//            _logger = logger;
//        }

//        public IActionResult Index()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }

//    }
//}
