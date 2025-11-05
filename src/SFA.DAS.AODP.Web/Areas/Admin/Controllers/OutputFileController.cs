using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Models.OutputFile;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PolicyConstants.IsAdminImportUser)]
    public class OutputFileController : ControllerBase
    {
        public const string OutputFileFailed = "OutputFileFailed";
        public const string OutputFileDefaultErrorMessage =
            @"Something went wrong while generating the output file.

            Please try again later. If the problem continues, contact support.";
        public const string OutputFileNoDataErrorMessage =
            @"There is no data available to create an output file for this cycle because no applications have been reviewed yet. 

            Try again when you've processed one or more applications.";

        public OutputFileController(ILogger<OutputFileController> logger, IMediator mediator) : base(mediator, logger)
        { }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logsEnvelope = await _mediator.Send(new GetQualificationOutputFileLogQuery());
            var logs = logsEnvelope.Value?.OutputFileLogs ?? Enumerable.Empty<GetQualificationOutputFileLogResponse.QualificationOutputFileLog>();
            var viewModel = new OutputFileViewModel
            {
                OutputFileLogs = logs.Select(x => new OutputFileLogModel
                {
                    UserDisplayName = x.UserDisplayName,
                    Timestamp = x.Timestamp
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet("outputfile")]
        public async Task<IActionResult> GetOutputFile()
        { 
            var result = await _mediator.Send(new GetQualificationOutputFileQuery(HttpContext.User?.Identity?.Name!));

            if (result == null || !result.Success)
            {
                TempData[OutputFileFailed] = true;

                TempData[$"{OutputFileFailed}:Message"] =
                    result?.ErrorCode == ErrorCodes.NoData
                    ? OutputFileNoDataErrorMessage
                    : OutputFileDefaultErrorMessage;

                return RedirectToAction(nameof(Index));
            }

            var bytes = result.Value.ZipFileContent;
            var contentType = string.IsNullOrWhiteSpace(result.Value.ContentType) ? "application/zip" : result.Value.ContentType;
            var fileName = string.IsNullOrWhiteSpace(result.Value.FileName) ? "export.zip" : result.Value.FileName;

            return File(bytes, contentType, fileName);
        }
    }
}
