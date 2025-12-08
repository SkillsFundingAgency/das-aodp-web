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
public class ImportController : ControllerBase
{
    private const string DefundingListViewPath = "~/Areas/Import/Views/DefundingList/Index.cshtml";
    private const string ImportedViewPath = "~/Areas/Import/Views/Imported.cshtml";

    public ImportController(
        IMediator mediator,
        ILogger<ImportController> logger) : base(mediator, logger)
    {
    }

    [HttpGet]
    [Route("import/defunding-list")]
    public IActionResult ImportDefundingList()
    {
        return View(DefundingListViewPath);
    }

    [HttpPost]
    [Route("import/defunding-list")]
    [Consumes("multipart/form-data")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.QFAUImport)]
    public async Task<IActionResult> ImportDefundingList([FromForm] UploadImportFileViewModel model)
    {

        if (!ModelState.IsValid)
        {
            return View(DefundingListViewPath, model);
        }

        if (model.File == null
                || model.File.Length == 0
                || !Path.GetExtension(model.File.FileName ?? string.Empty).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.File), "You must select an .xlsx file.");
            return View(DefundingListViewPath, model);
        }

        try
        {
            var command = new ImportDefundingListCommand
            {
                File = model.File
            };

            await Send(command);

            ViewBag.Heading = "Defunding list imported";
            return View(ImportedViewPath);
        }
        catch (Exception ex)
        {
            LogException(ex);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while uploading the file. Please try again.");
            return View(DefundingListViewPath, model);
        }
    }
}