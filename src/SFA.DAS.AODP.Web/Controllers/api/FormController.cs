using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Domain.Forms.GetAllForms;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class FormController : ControllerBase
{
    private readonly IGenericRepository<Form> formRepository;

    public FormController(IGenericRepository<Form> f)
    {
        formRepository = f;
    }

    [HttpGet]
    [Route("/api/forms")]
    [ProducesResponseType(typeof(List<Form>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IActionResult GetForms()
    {
        try
        {
            var forms = formRepository.GetAll().ToList();

            var response = new GetAllFormsResponse()
            {
                Forms = forms
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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
