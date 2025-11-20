using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Import;
using SFA.DAS.AODP.Web.Areas.Import.Models;
using SFA.DAS.AODP.Web.Authentication;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Import.Controllers;

[Area("Import")]
[Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
[Route("import")]
public class ImportController : ControllerBase
{
    public ImportController(
        IMediator mediator,
        ILogger<ImportController> logger) : base(mediator, logger)
    {
    }

    [HttpGet("defunding-list")]
    public IActionResult Index()
    {
        return View("~/Areas/Import/Views/DefundingList/Index.cshtml");
    }

    [HttpPost("defunding-list")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index([FromForm] UploadDefundingListViewModel model)
    {

        if (!ModelState.IsValid)
        {
            return View("~/Areas/Import/Views/DefundingList/Index.cshtml", model);
        }

        if (model.File == null
                || model.File.Length == 0
                || !Path.GetExtension(model.File.FileName ?? string.Empty).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.File), "You must select an .xlsx file.");
            return View("~/Areas/Import/Views/DefundingList/Index.cshtml", model);
        }

        try
        {
            var command = new ImportDefundingListCommand
            {
                File = model.File
            };

            await Send(command);

            return View("~/Areas/Import/Views/DefundingList/Imported.cshtml");
        }
        catch (Exception ex)
        {
            LogException(ex);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while uploading the file. Please try again.");
            return View(model);
        }
    }
}