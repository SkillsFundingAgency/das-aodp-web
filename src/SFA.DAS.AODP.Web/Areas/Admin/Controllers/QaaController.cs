using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.QaaDownload;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Models.Qaa;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PolicyConstants.IsAdminImportUser)]
    public class QaaController : ControllerBase
    {
        public QaaController(ILogger<QaaController> logger, IMediator mediator) : base(mediator, logger)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var summary = await Send(new GetQaaDownloadSummaryQuery());

            var viewModel = new QaaViewModel
            {
                DataLastImportedDate = summary.DataLastImportedDate,
                MostRecentDownloadDate = summary.MostRecentDownloadDate,
                NewQualificationsCount = summary.NewQualificationsCount,
                ExtendedQualificationsCount = summary.ExtendedQualificationsCount,
                DiscontinuedQualificationsCount = summary.DiscontinuedQualificationsCount,
                DownloadHistory = summary.DownloadHistory.Select(x => new QaaDownloadLogModel
                {
                    UserDisplayName = x.UserDisplayName,
                    DownloadDate = x.DownloadDate
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Download()
        {
            var file = await Send(new GetQaaQualificationsExportQuery
            {
                CurrentUsername = HttpContext.User?.Identity?.Name ?? string.Empty
            });

            return File(file.FileContent, file.ContentType, file.FileName);
        }
    }
}
