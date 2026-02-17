using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Authentication;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

[Area("Review")]
[Route("{controller}/{action}")]
[Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
public class RolloverController : ControllerBase
{
    private readonly ILogger<RolloverController> _logger;

    public RolloverController(ILogger<RolloverController> logger, IMediator mediator) : base(mediator, logger)
    {
        _logger = logger;
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
    [Route("/Review/Rollover/FundingStreamInclusionExclusion")]
    public IActionResult FundingStreamInclusionExclusion()
    {
        ViewData["Title"] = "Select funding stream(s)";

        var vm = new FundingStreamInclusionExclusionViewModel
        {
            FundingStreams = GetFundingStreams()
        };

        return View(vm);
    }

    [HttpPost]
    [Route("/Review/Rollover/FundingStreamInclusionExclusion")]
    public IActionResult FundingStreamInclusionExclusion(FundingStreamInclusionExclusionViewModel vm, string action)
    {
        var fundingStreams = GetFundingStreams();
        var validIds = fundingStreams.Select(x => x.Id).ToHashSet();

        vm.FundingStreams = fundingStreams;

        if (action == "selectAll")
        {
            vm.SelectedIds = validIds.ToList();
            ModelState.Clear();
            return View(vm);
        }

        if (vm.SelectedIds == null || !vm.SelectedIds.Any())
        {
            ModelState.AddModelError(nameof(vm.SelectedIds), "Select at least one funding stream");
            return View(vm);
        }

        if (!vm.SelectedIds.All(id => validIds.Contains(id)))
        {
            ModelState.AddModelError(string.Empty, "Invalid selection");
            return View(vm);
        }

        return RedirectToAction(nameof(EnterRolloverEligibilityDates));
    }

    [HttpGet]
    [Route("/Review/Rollover/EnterRolloverEligibilityDates")]
    public IActionResult EnterRolloverEligibilityDates() => View();

    private List<FundingStream> GetFundingStreams()
    {
        return new List<FundingStream>
        {
            new FundingStream { Id = 1, Label = "Age 14-16" },
            new FundingStream { Id = 2, Label = "Age 16-19" },
            new FundingStream { Id =3, Label = "Local flexibilities" },
            new FundingStream { Id = 4, Label = "Legal entitlement L2 L3" },
            new FundingStream { Id = 5, Label = "Legal entitlement English and Maths" },
            new FundingStream { Id = 6, Label = "Digital entitlement" },
            new FundingStream { Id = 7, Label = "Lifelong learning entitlement" },
            new FundingStream { Id = 8, Label = "Advanced learner loans" },
            new FundingStream { Id = 9, Label = "Free courses for jobs" }
        };
    }
}