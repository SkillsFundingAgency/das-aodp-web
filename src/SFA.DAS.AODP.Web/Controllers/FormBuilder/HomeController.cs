using System.Diagnostics;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models;

namespace SFA.DAS.AODP.Web.Controllers.FormBuilder
{

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
    }
}
