using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Models.OutputFile;
using System;
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
        { }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
    var logs    = logsEnvelope.Value?.OutputFileLogs ?? Enumerable.Empty<GetQualificationOutputFileLogResponse.QualificationOutputFileLog>();
            ShowNotificationIfKeyExists(
                OutputFileFailed,
                ViewNotificationMessageType.Error,
                TempData[$"{OutputFileFailed}:Message"] as string);

           

            var viewModel = new OutputFileViewModel
            {
                OutputFileLogs = logs.Select(x => new OutputFileLogModel
                {
                    UserDisplayName = x.UserDisplayName,
                    Timestamp = x.Timestamp

            };

            return View(viewModel);
        }

        [HttpGet("outputfile")]
        public async Task<IActionResult> GetOutputFile(CancellationToken cancellationToken)
        { 
            var result = await _mediator.Send(new GetQualificationOutputFileQuery { CurrentUsername = HttpContext.User?.Identity?.Name! }, cancellationToken);

            if (!result.Success || result.Value is null || result.Value.ZipFileContent is null || result.Value.ZipFileContent.Length == 0)
            {
                TempData[OutputFileFailed] = true;
                TempData[$"{OutputFileFailed}:Message"] = string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? OutputFileDefaultErrorMessage
                    : result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            var bytes = result.Value.ZipFileContent;
            var contentType = string.IsNullOrWhiteSpace(result.Value.ContentType) ? "application/zip" : result.Value.ContentType;
            var fileName = string.IsNullOrWhiteSpace(result.Value.FileName) ? "export.zip" : result.Value.FileName;

            return File(bytes, contentType, fileName);
        }
    }
}
