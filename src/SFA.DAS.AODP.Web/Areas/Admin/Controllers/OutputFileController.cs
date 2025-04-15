using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.OutputFile;
using SFA.DAS.AODP.Application.Queries.OutputFile;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Models.OutputFile;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PolicyConstants.IsAdminImportUser)]
public class OutputFileController : ControllerBase
{
    public OutputFileController(IMediator mediator, ILogger<FormsController> logger) : base(mediator, logger)
    { }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await Send(new GetPreviousOutputFilesQuery());
            GenerateViewModel model = response;
            return View(model);
        }
        catch (Exception ex)
        {
            LogException(ex);
            return Redirect("/Home/Error");
        }
    }
    [HttpPost]
    public async Task<IActionResult> Index([FromBody]GenerateViewModel model)
    {
        try
        {
            if (model.AdditionalFormActions.GenerateFile)
            {
                await Send(new GenerateNewOutputFileCommand());
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }
}
