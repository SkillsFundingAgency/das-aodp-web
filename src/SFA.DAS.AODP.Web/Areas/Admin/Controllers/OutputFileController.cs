using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Models.OutputFile;
using System;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PolicyConstants.IsAdminImportUser)]
    public class OutputFileController : ControllerBase
    {
        public OutputFileController(ILogger<OutputFileController> logger, IMediator mediator) : base(mediator, logger)
        { }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var logsEnvelope = await _mediator.Send(new GetQualificationOutputFileLogQuery(), ct);
            var logs = logsEnvelope.Value?.OutputFileLogs ?? Enumerable.Empty<GetQualificationOutputFileLogResponse.QualificationOutputFileLog>();

            var orderedLogs = logs
                .Select(x => new OutputFileLogModel
                {
                    UserDisplayName = x.UserDisplayName,
                    Timestamp = x.Timestamp
                })
                .ToList();

            var viewModel = new OutputFileViewModel
            {
                OutputFileLogs = orderedLogs
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> StartDownload(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetQualificationOutputFileQuery(HttpContext.User?.Identity?.Name!), ct);

            if (!result.Success || result.Value is null)
            {
                return Problem(result.ErrorMessage ?? "Unable to retrieve output file.");
            }

            var bytes = result.Value.ZipFileContent;
            var contentType = string.IsNullOrWhiteSpace(result.Value.ContentType) ? "application/zip" : result.Value.ContentType;
            var fileName = string.IsNullOrWhiteSpace(result.Value.FileName) ? "export.zip" : result.Value.FileName;

            return File(bytes, contentType, fileName);
        }
    }
}
