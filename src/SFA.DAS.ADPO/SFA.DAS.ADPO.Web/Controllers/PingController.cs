using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.ADPO.Infrastructure.ApiClients;

namespace SFA.DAS.ADPO.Web.Controllers
{
    public class PingController : Controller
    {
        [AllowAnonymous]
        [HttpGet("/Ping")]
        public IActionResult Ping()
        {
            return Ok("Pong");
        }
    }
}
