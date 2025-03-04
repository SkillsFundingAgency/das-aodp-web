using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Globalization;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    public class ChangedQualificationDetailsController : Controller
    {
        private readonly ILogger<QualificationsController> _logger;
        private readonly IMediator _mediator;

        public ChangedQualificationDetailsController(ILogger<QualificationsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

    }
}
