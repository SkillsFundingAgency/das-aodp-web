using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Controllers
{
    public class ErrorController: Controller
    {
        [Route("error/403")]
        public  IActionResult NotAuthorised()
        {
            return View();
        }

        [Route("error/404")]
        public IActionResult NotFound()
        {
            return View();
        }

        [Route("error/400")] 
        public IActionResult BadRequest()
        { 
            return View("Error"); 
        }
    }
}
