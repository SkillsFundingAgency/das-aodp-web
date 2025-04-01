using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Filters;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Models.Survey;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Apply.Controllers
{
    [Area("Apply")]
    [Authorize(Policy = PolicyConstants.IsApplyUser)]
    [ValidateOrganisation]
    public class SurveyController : ControllerBase
    {
        public SurveyController(IMediator mediator, ILogger<SurveyController> logger) : base(mediator, logger)
        {
        }

        [HttpGet]
        [Route("/survey/{page}")]
        public async Task<IActionResult> SurveyFeedback(string page)
        {
            var model = new SurveyViewModel
            {
                Page = page
            };

            return View(model);
        }

        [HttpPost]
        [Route("/survey/{page}")]
        public async Task<IActionResult> SurveyFeedback(SurveyViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (viewModel.SatisfactionScore == null)
            {
                ModelState.AddModelError(nameof(viewModel.SatisfactionScore), "Satisfaction score is required.");
                return View(viewModel);
            }

            try
            {
                await Send(new SaveSurveyCommand()
                {
                    Page = viewModel.Page,
                    SatisfactionScore = (int)viewModel.SatisfactionScore,
                    Comments = viewModel.Comments
                });
                return RedirectToAction(nameof(SurveyFeedbackConfirmation));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return View(viewModel);
            }
        }

        [HttpGet]
        [Route("/survey/feedbackConfirmation")]
        public async Task<IActionResult> SurveyFeedbackConfirmation()
        {
            return View();
        }
    }
}


