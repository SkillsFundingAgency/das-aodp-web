using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFA.DAS.AODP.Infrastructure.MemoryCache;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Controllers.api;

[Route("api/[controller]")]
[ApiController]
public class SectionController : BaseApiController
{
    public SectionController(ICacheManager cacheManager) : base(cacheManager) { }

    [HttpGet]
    [Route("GetSections", Name = "GetSections")]
    [ProducesResponseType(typeof(List<Section>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public IActionResult GetSections(Guid formId)
    {
        try
        {
            var sections = GetFromCache<List<Section>>("Sections")?.Where(s => s.FormId == formId).ToList() ?? new List<Section>();
            return Ok(sections);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
