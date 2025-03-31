using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Filters;
using SFA.DAS.AODP.Web.Models.ExitSurvey;
using SFA.DAS.AODP.Web.Models.Qualifications.Fundings;
using System.Reflection;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Apply.Controllers
{
    [Area("Apply")]
    [Authorize(Policy = PolicyConstants.IsApplyUser)]
    [ValidateOrganisation]
    public class ExitSurveyController : ControllerBase
    {
        public ExitSurveyController(IMediator mediator, ILogger<ExitSurveyController> logger) : base(mediator, logger)
        {
        }

        [HttpGet]
        [Route("/survey/{page}")]
        public async Task<IActionResult> ExitSurveyFeedback(string page)
        {
            var model = new ExitSurveyFeedbackViewModel
            {
                Page = page
            };

            return View(model);
        }

        [HttpPost]
        [Route("/survey/{page}")]
        public async Task<IActionResult> ExitSurveyFeedback(ExitSurveyFeedbackViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            //Save feedback

            try
            {
                return RedirectToAction(nameof(ExitSurveyFeedbackConfirmation));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return View(viewModel);
            }
        }

        [HttpGet]
        [Route("/survey/feedbackConfirmation")]
        public async Task<IActionResult> ExitSurveyFeedbackConfirmation()
        {
            return View();
        }
    }
}


