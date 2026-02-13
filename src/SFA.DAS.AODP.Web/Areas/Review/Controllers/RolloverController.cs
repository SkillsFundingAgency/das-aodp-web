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
    private readonly IValidator<RolloverFundingApprovalEndDateViewModel> _rolloverFundingApprovalEndDateViewModelViewModeValidator;

    public RolloverController(ILogger<RolloverController> logger, IMediator mediator, IValidator<RolloverFundingApprovalEndDateViewModel> validator) : base(mediator, logger)
    {
        _logger = logger;
        _rolloverFundingApprovalEndDateViewModelViewModeValidator = validator;
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
    [Route("/Review/Rollover/EnterRolloverFundingApprovalEndDate")]
    public IActionResult EnterRolloverFundingApprovalEndDate()
    {
        ViewData["Title"] = "Set the end date for funding extension";
        return View();
    }

    [HttpPost]
    [Route("/Review/Rollover/EnterRolloverFundingApprovalEndDate")]
    public async Task<IActionResult> EnterRolloverFundingApprovalEndDate(RolloverFundingApprovalEndDateViewModel model)
    {
        var validation = await _rolloverFundingApprovalEndDateViewModelViewModeValidator.ValidateAsync(model);

        validation.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            return View("EnterRolloverFundingApprovalEndDate", model);
        }

        return RedirectToAction(nameof(EnterRolloverFundingApprovalEndDate));
    }
}
