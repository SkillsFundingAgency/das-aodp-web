using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Extensions;
using System.Diagnostics.CodeAnalysis;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

[Area("Review")]
[Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
public class RolloverController : ControllerBase
{
    private readonly ILogger<RolloverController> _logger;
    private const string SessionKey = "RolloverSession";
    private readonly IValidator<RolloverEligibilityDatesViewModel> _rolloverEligibilityDatesViewModeValidator;
    private readonly IValidator<RolloverFundingApprovalEndDateViewModel> _rolloverFundingApprovalEndDateViewModelViewModeValidator;
    private const string RolloverStartView = "RolloverStart";
    public RolloverController(ILogger<RolloverController> logger, IMediator mediator, IValidator<RolloverEligibilityDatesViewModel> validatorEligibilityDates, IValidator<RolloverFundingApprovalEndDateViewModel> validatorApprovalEndDate) : base(mediator, logger)
    {
        _logger = logger;
        _rolloverEligibilityDatesViewModeValidator = validatorEligibilityDates;
        _rolloverFundingApprovalEndDateViewModelViewModeValidator = validatorApprovalEndDate;
    }

    [HttpGet]
    [Route("/Review/Rollover")]
    public IActionResult Index()
    {
        var session = GetSessionModel();
        var model = session.Start != null
            ? new RolloverStartViewModel { SelectedProcess = session.Start.SelectedProcess }
            : new RolloverStartViewModel();

        return View(RolloverStartView, model);
    }

    [HttpPost]
    [Route("/Review/Rollover")]
    public IActionResult Index(RolloverStartViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("RolloverStart", model);
        }

        var session = GetSessionModel();
        (session.Start ??= new RolloverStart()).SetStart(session, model);
        SaveSessionModel(session);

        return model.SelectedProcess switch
        {
            RolloverProcess.InitialSelection => RedirectToAction(nameof(CheckData)),
            RolloverProcess.FinalUpload => RedirectToAction(nameof(UploadQualifications)),
            _ => View(RolloverStartView, model)
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
    [Route("/Review/Rollover/CheckData")]
    public async Task<IActionResult> CheckData()
    {
        var session = GetSessionModel();
        if (session.ImportStatus != null)
        {
            var vm = RolloverImportStatusViewModel.MapFromSession(session.ImportStatus);

            ViewData["Title"] = "Do you need to update any data before starting?";
            return View("CheckData", vm);
        }

        var model = new RolloverImportStatusViewModel();

        try
        {
            var regulatedResp = await Send(new GetJobRunsQuery { JobName = JobNames.RegulatedQualifications.ToString() });
            if (regulatedResp?.JobRuns != null && regulatedResp.JobRuns.Any())
            {
                var latest = regulatedResp.JobRuns
                    .OrderByDescending(j => j.EndTime ?? DateTime.MinValue)
                    .FirstOrDefault();
                model.RegulatedQualificationsLastImported = latest?.EndTime ?? latest?.StartTime;
            }

            var fundedResp = await Send(new GetJobRunsQuery { JobName = JobNames.FundedQualifications.ToString() });
            if (fundedResp?.JobRuns != null && fundedResp.JobRuns.Any())
            {
                var latest = fundedResp.JobRuns
                    .OrderByDescending(j => j.EndTime ?? DateTime.MinValue)
                    .FirstOrDefault();
                model.FundedQualificationsLastImported = latest?.EndTime ?? latest?.StartTime;
            }

            var defundingResp = await Send(new GetJobRunsQuery { JobName = JobNames.DefundingList.ToString() });
            if (defundingResp?.JobRuns != null && defundingResp.JobRuns.Any())
            {
                var latest = defundingResp.JobRuns
                    .OrderByDescending(j => j.EndTime ?? DateTime.MinValue)
                    .FirstOrDefault();
                model.DefundingListLastImported = latest?.EndTime ?? latest?.StartTime;
            }

            var pldnsResp = await Send(new GetJobRunsQuery { JobName = JobNames.Pldns.ToString() });
            if (pldnsResp?.JobRuns != null && pldnsResp.JobRuns.Any())
            {
                var latest = pldnsResp.JobRuns
                    .OrderByDescending(j => j.EndTime ?? DateTime.MinValue)
                    .FirstOrDefault();
                model.PldnsListLastImported = latest?.EndTime ?? latest?.StartTime;
            }

            (session.ImportStatus ??= new RolloverImportStatus()).SetImportStatus(session, model);
            SaveSessionModel(session);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        ViewData["Title"] = "Do you need to update any data before starting?";
        return View("CheckData", model);
    }

    [HttpPost]
    [Route("/Review/Rollover/CheckData")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckData([FromForm] RolloverImportStatusViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var session = GetSessionModel();

            var vm = session.ImportStatus != null
                ? RolloverImportStatusViewModel.MapFromSession(session.ImportStatus)
                : model;

            return View("CheckData", vm);
        }

        var sessionModel = GetSessionModel();

        var sessionCountAvailable = sessionModel.PreviousData != null && sessionModel.PreviousData.CandidateCount > 0;
        var count = sessionCountAvailable ? sessionModel.PreviousData.CandidateCount : 0;

        if (!sessionCountAvailable)
        {
            var candidateCount = await Send(new GetRolloverWorkflowCandidatesCountQuery());
            count = candidateCount.TotalRecords;

            if (count > 0)
            {
                try
                {
                    var previousData = new RolloverPreviousDataViewModel
                    {
                        CandidateCount = count
                    };
                    (sessionModel.PreviousData ??= new RolloverPreviousData()).SetPreviousDataCandidate(sessionModel, previousData);
                    SaveSessionModel(sessionModel);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }
        }

        if (count > 0)
        {
            return RedirectToAction(nameof(PreviousFile));
        }

        return RedirectToAction(nameof(SelectCandidates), new { returnAction = nameof(CheckData) });
    }

    [HttpGet]
    [Route("/Review/Rollover/PreviousFile")]
    public async Task<IActionResult> PreviousFile()
    {
        var session = GetSessionModel();

        if (session.PreviousData != null)
        {
            var vm = new RolloverPreviousDataViewModel
            {
                CandidateCount = session.PreviousData.CandidateCount,
                SelectedOption = session.PreviousData.SelectedOption
            };

            return View("PreviousFile", vm);
        }

        var candidateCount = await Send(new GetRolloverWorkflowCandidatesCountQuery());

        var model = new RolloverPreviousDataViewModel
        {
            CandidateCount = candidateCount.TotalRecords
        };

        (session.PreviousData ??= new RolloverPreviousData()).SetPreviousDataCandidate(session, model);
        SaveSessionModel(session);

        return View("PreviousFile", model);
    }

    [HttpPost]
    [Route("/Review/Rollover/PreviousFile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreviousFile(RolloverPreviousDataViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("PreviousFile", model);
        }

        var session = GetSessionModel();
        try
        {
            (session.PreviousData ??= new RolloverPreviousData()).SetPreviousDataCandidate(session, model);
            SaveSessionModel(session);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return model.SelectedOption switch
        {
            RolloverPreviousFileOption.ContinueProcessing => RedirectToAction(nameof(SelectFundingStreams)),
            RolloverPreviousFileOption.RemovePrevious => RedirectToAction(nameof(SelectCandidates), new { returnAction = nameof(PreviousFile) }),
            _ => View(RolloverStartView, model)
        };
    }

    [HttpGet]
    [Route("/Review/Rollover/SelectCandidates")]
    public IActionResult SelectCandidates([FromQuery] string? returnAction = null)
    {
        var session = GetSessionModel();
        var model = new RolloverSelectCandidatesViewModel();

        if (session.SelectCandidates != null)
        {
            model.SelectedOption = session.SelectCandidates.SelectedOption;
            model.ReturnUrl ??= returnAction ?? session.SelectCandidates.ReturnUrl;
        }
        else
        {
            model.ReturnUrl ??= returnAction ?? nameof(CheckData);
        }
        
        return View("SelectCandidates", model);
    }

    [HttpPost]
    [Route("/Review/Rollover/SelectCandidates")]
    [ValidateAntiForgeryToken]
    public IActionResult SelectCandidates([FromForm] RolloverSelectCandidatesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("SelectCandidates", model);
        }

        var session = GetSessionModel();
        (session.SelectCandidates ??= new RolloverSelectCandidates()).SetSelectCandidates(session, model);
        SaveSessionModel(session);

        return model.SelectedOption switch
        {
            SelectCandidatesForRollover.ImportAList => RedirectToAction(nameof(ImportCandidatesList)),
            SelectCandidatesForRollover.GenerateAList  => RedirectToAction(nameof(RolloverQueryBuilder)),
            _ => View()
        };
    }

    [HttpGet]
    [Route("/Review/Rollover/ImportCandidatesList")]
    public IActionResult ImportCandidatesList()
    {
        ViewData["Title"] = "Import Candidates List ";
        return View();
    }

    [HttpGet]
    [Route("/Review/Rollover/RolloverQueryBuilder")]
    public IActionResult RolloverQueryBuilder()
    {
        ViewData["Title"] = "Rollover Query Builder";
        return View();
    }

    [HttpGet]
    [Route("/Review/Rollover/SelectFundingStreams")]
    public IActionResult SelectFundingStreams()
    {
        ViewData["Title"] = "Select funding stream(s)";
        return View();
    }

    [HttpGet]
    [Route("/Review/Rollover/UploadQualificationCandidates")]
    public IActionResult UploadQualificationCandidates()
    {
        return View(new RolloverUploadQualificationCandidatesViewModel());
    }

    [HttpPost]
    [Route("/Review/Rollover/UploadQualificationCandidates")]
    public async Task<IActionResult> UploadQualificationCandidates([FromForm] RolloverUploadQualificationCandidatesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction("FundingStreamInclusionExclusion");
    }

    [ExcludeFromCodeCoverage]
    [HttpGet]
    [Route("/Review/Rollover/FundingStreamInclusionExclusion")]
    public IActionResult FundingStreamInclusionExclusion()
    {
        var vm = new FundingStreamInclusionExclusionViewModel
        {
            FundingStreams = GetFundingStreams()
        };

        return View(vm);
    }

    [ExcludeFromCodeCoverage]
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
            ModelState.AddModelError(nameof(vm.SelectedIds), "Select at least one funding stream.");
            return View(vm);
        }

        if (!vm.SelectedIds.All(id => validIds.Contains(id)))
        {
            ModelState.AddModelError(string.Empty, "Invalid selection");
            return View(vm);
        }

        return RedirectToAction(nameof(EnterRolloverEligibilityDates));
    }

    [ExcludeFromCodeCoverage]
    [HttpGet]
    [Route("/Review/Rollover/EnterRolloverEligibilityDates")]
    public IActionResult EnterRolloverEligibilityDates() => View();

    [ExcludeFromCodeCoverage]
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

    private Rollover GetSessionModel()
    {
        try
        {
            var model = HttpContext.Session.GetObject<Rollover>(SessionKey);
            if (model == null) model = new Rollover();
            return model;
        }
        catch (Exception ex)
        {
            LogException(ex);
            return new Rollover();
        }
    }

    private void SaveSessionModel(Rollover model)
    {
        try
        {
            HttpContext.Session.SetObject(SessionKey, model);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }
}
