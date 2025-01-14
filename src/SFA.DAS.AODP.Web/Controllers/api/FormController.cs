using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFA.DAS.AODP.Models.Forms.DTO;

namespace SFA.DAS.AODP.Web.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class FormController : ControllerBase
{
    public FormController() { }

    [AllowAnonymous]
    [HttpPost("ping")]
    [IgnoreAntiforgeryToken]
    public IActionResult Ping()
    {
        return Ok("pong");
    }

    [AllowAnonymous]
    [HttpPost("Submit")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Submit()
    {
        try
        {
            StreamReader requestReader = new StreamReader(Request.Body);
            var data = await requestReader.ReadToEndAsync();
            var parsed = JsonConvert.DeserializeObject<Form>(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Created();
    }
}
