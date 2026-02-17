using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Extensions;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

[Area("Review")]
[Route("{controller}/{action}")]
[Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
public class RolloverController : ControllerBase
{
    private readonly ILogger<RolloverController> _logger;
    private readonly IValidator<RolloverEligibilityDatesViewModel> _rolloverEligibilityDatesViewModeValidator;

    public RolloverController(ILogger<RolloverController> logger, IMediator mediator, IValidator<RolloverEligibilityDatesViewModel> validator) : base(mediator, logger)
    {
        _logger = logger;
        _rolloverEligibilityDatesViewModeValidator = validator;
    }

    [HttpGet]
    [Route("/Review/Rollover")]
    public IActionResult Index()
    {
        var model = new RolloverStartViewModel();
        return View("RolloverStart", model);
    }

    [HttpPost]
    [Route("/Review/Rollover")]
    public IActionResult Index(RolloverStartViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("RolloverStart", model);
        }

        return model.SelectedProcess switch
        {
            RolloverProcess.InitialSelection => RedirectToAction(nameof(InitialSelection)),
            RolloverProcess.FinalUpload => RedirectToAction(nameof(UploadQualifications)),
            _ => View("RolloverStart", model)
        };
    }

    [HttpGet]
    [Route("/Review/Rollover/InitialSelection")]
    public IActionResult InitialSelection()
    {
        ViewData["Title"] = "Initial selection of qualificaton";
        return View();
    }

    [HttpGet]
    [Route("/Review/Rollover/UploadQualifications")]
    public IActionResult UploadQualifications()
    {
        ViewData["Title"] = "Upload qualifications to RollOver";
        return View();
    }

    [HttpGet]
    [Route("/Review/Rollover/EnterRolloverEligibilityDates")]
    public IActionResult EnterRolloverEligibilityDates()
    {
        ViewData["UseNewLayout"] = true;
        ViewData["Title"] = "Enter rollover eligibility dates";

        return View();
    }

    [HttpPost]
    [Route("/Review/Rollover/EnterRolloverEligibilityDates")]
    public async Task<IActionResult> EnterRolloverEligibilityDates(RolloverEligibilityDatesViewModel model)
    {
        ViewData["UseNewLayout"] = true;

        var validation = await _rolloverEligibilityDatesViewModeValidator.ValidateAsync(model);
        validation.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            return View("EnterRolloverEligibilityDates", model);
        }

        return View("EnterMaximumFundingApprovalEndDate", model);
    }
}