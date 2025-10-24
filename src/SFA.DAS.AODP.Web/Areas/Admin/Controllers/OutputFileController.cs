using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PolicyConstants.IsAdminImportUser)]
    public class OutputFileController : ControllerBase
    {
        public const string OutputFileFailed = "OutputFileFailed";
        public const string OutputFileDefaultErrorMessage = "Error downloading output file";

        public OutputFileController(ILogger<OutputFileController> logger, IMediator mediator) : base(mediator, logger)
        { 
        }

        public async Task<IActionResult> Index()
        {
            ShowNotificationIfKeyExists(
                OutputFileFailed,
                ViewNotificationMessageType.Error,
                TempData[$"{OutputFileFailed}:Message"] as string);

            return View();
        }

        [HttpGet("outputfile")]
        public async Task<IActionResult> GetOutputFile(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetQualificationOutputFileQuery(), cancellationToken);

            if (!result.Success || result.Value is null || result.Value.ZipFileContent is null || result.Value.ZipFileContent.Length == 0)
            {
                TempData[OutputFileFailed] = true;
                TempData[$"{OutputFileFailed}:Message"] = string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? OutputFileDefaultErrorMessage
                    : result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            return File(
                fileContents: result.Value.ZipFileContent,
                contentType: result.Value.ContentType,
                fileDownloadName: result.Value.FileName
            );
        }
    }
}
