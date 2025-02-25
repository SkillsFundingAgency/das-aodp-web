using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models;


namespace SFA.DAS.AODP.Web.Areas.Apply.Controllers
{
    [Area("Apply")]
    public class ApplyController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public ApplyController(ILogger<HomeController> logger)
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
    }
}
