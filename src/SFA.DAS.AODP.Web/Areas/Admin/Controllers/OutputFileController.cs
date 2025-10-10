using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PolicyConstants.IsAdminImportUser)]
    public class OutputFileController : ControllerBase
    {
        public OutputFileController(ILogger<OutputFileController> logger, IMediator mediator) : base(mediator, logger)
        { 
        }


        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet("export")]
        public async Task<IActionResult> GetExport(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetQualificationExportFileQuery(), cancellationToken);

            if (!result.Success || result.Value is null)
            {
                return Problem(result.ErrorMessage ?? "Unable to retrieve export file.");
            }

            return File(
                fileContents: result.Value.ZipFileContent,
                contentType: result.Value.ContentType,
                fileDownloadName: result.Value.FileName
            );
        }
    }
}
