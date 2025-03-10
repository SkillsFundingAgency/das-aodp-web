using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.AODP.Web.Controllers
{
    public class PingControllerTests : Controller
    {
        [AllowAnonymous]
        [HttpGet("/Ping")]
        public IActionResult Ping()
        {
            return Ok("Pong");
        }
    }
}
