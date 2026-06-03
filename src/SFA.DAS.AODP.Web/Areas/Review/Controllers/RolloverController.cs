using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Rollover;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Application.Validators;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Extensions;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Helpers.User;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

[Area("Review")]
[Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
public class RolloverController : ControllerBase
{
    private readonly ILogger<RolloverController> _logger;
    private const string SessionKey = "RolloverSession";
    private const string RolloverStartView = "RolloverStart";
    private readonly ICsvFileReader _csvFileReader;
    private readonly IValidator<RolloverEligibilityDatesViewModel> _rolloverEligibilityDatesViewModeValidator;
    private readonly IValidator<RolloverFundingApprovalEndDateViewModel> _rolloverFundingApprovalEndDateViewModelViewModeValidator;
    private readonly IUserHelperService _userHelperService;

    public RolloverController(ILogger<RolloverController> logger,
        IMediator mediator,
        IValidator<RolloverEligibilityDatesViewModel> validatorEligibilityDates,
        IValidator<RolloverFundingApprovalEndDateViewModel> validatorApprovalEndDate,
        ICsvFileReader csvFileReader,
        IUserHelperService userHelperService) : base(mediator, logger)
    {
        _logger = logger;
        _rolloverEligibilityDatesViewModeValidator = validatorEligibilityDates;
        _rolloverFundingApprovalEndDateViewModelViewModeValidator = validatorApprovalEndDate;
        _csvFileReader = csvFileReader;
        _userHelperService = userHelperService;
    }

    [HttpGet]
    [Route("/Review/Rollover")]
    public IActionResult Index()
    {
        var session = GetSessionModel();
        var model = session.Start != null
            ? new RolloverStartViewModel { SelectedProcess = session.Start.SelectedProcess }
            : new RolloverStartViewModel();

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

        var session = GetSessionModel();
        (session.Start ??= new RolloverStart()).SetStart(session, model);
        SaveSessionModel(session);

        return model.SelectedProcess switch
        {
            RolloverProcess.InitialSelection => RedirectToAction(nameof(CheckData)),
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
        return View();
    }

    [HttpPost]
    [Route("/Review/Rollover/UploadQualifications")]
    public async Task<IActionResult> UploadQualifications([FromForm] RolloverUploadQualificationsViewModel model)
    {
        var session = GetSessionModel();

        if (model.File == null && session.RolloverFundingExtensionCandidates != null)
        {
            return RedirectToAction("RolloverSummary");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var file = await _csvFileReader.FileReadAsync(
            model.File,
            FundingExtensionCandidateColumns.Required,
            FundingExtensionCandidateMapper.Map
        );

        if (!file.IsValid)
        {
            foreach (var error in file.Errors)
                ModelState.AddModelError(nameof(model.File), error);

            return View(model);
        }

        var responseRolloverCandidates = new GetRolloverCandidatesQueryResponse();
        var responseRolloverWorkflowCandidates = new GetRolloverWorkflowCandidatesQueryResponse();

        try
        {
            responseRolloverCandidates = await Send(new GetRolloverCandidatesQuery());
            responseRolloverWorkflowCandidates = await Send(new GetRolloverWorkflowCandidatesQuery());

            var candidates = new List<AODP.Models.Rollover.FundingExtensionCandidate>();
            var rolloverCandidates = responseRolloverCandidates.RolloverCandidates;
            var rolloverWorkflowCandidates = responseRolloverWorkflowCandidates.RolloverWorkflowCandidates;
            var rolloverRun = new RolloverWorkflowRun
            { 
                WorkflowRunId = responseRolloverWorkflowCandidates.WorkflowRunId,
                FundingEndDateEligibilityThreshold = responseRolloverWorkflowCandidates.FundingEndDateEligibilityThreshold,
                MaximumApprovalFundingEndDate = responseRolloverWorkflowCandidates.MaximumApprovalFundingEndDate,
                OperationalEndDateEligibilityThreshold = responseRolloverWorkflowCandidates.OperationalEndDateEligibilityThreshold
            };
            var result = RolloverUploadQualificationsValidator.Validate(candidates, rolloverCandidates, rolloverWorkflowCandidates, rolloverRun);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        //var matchedCsv = RolloverCandidateExtensions.FilterCandidates(file.Items, response.RolloverCandidates);

        session.RolloverFundingExtensionCandidates = new();

        SaveSessionModel(session);

        return RedirectToAction("RolloverSummary");
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
            _ => View("RolloverStart", model)
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
            SelectCandidatesForRollover.ImportAList => RedirectToAction(nameof(UploadQualificationCandidates)),
            SelectCandidatesForRollover.GenerateAList  => RedirectToAction(nameof(RolloverQueryBuilder)),
            _ => View()
        };
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
    public async Task<IActionResult> UploadQualificationCandidates()
    {
        return View(new RolloverUploadQualificationCandidatesViewModel());
    }

    [HttpPost]
    [Route("/Review/Rollover/UploadQualificationCandidates")]
    public async Task<IActionResult> UploadQualificationCandidates([FromForm] RolloverUploadQualificationCandidatesViewModel model)
    {
        var session = GetSessionModel();

        if (model.File == null && session.RolloverCandidates.Any())
        {
            return RedirectToAction("FundingStreamInclusionExclusion");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var file = await _csvFileReader.FileReadAsync(
            model.File,
            QualificationImportColumns.Required,
            QualificationCandidateMapper.Map
        );

        if (!file.IsValid)
        {
            foreach (var error in file.Errors)
                ModelState.AddModelError(nameof(model.File), error);

            return View(model);
        }

        var response = new GetRolloverCandidatesQueryResponse();

        try
        {
            response = await Send(new GetRolloverCandidatesQuery());
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        var matchedCsv = RolloverCandidateExtensions.FilterCandidates(file.Items, response.RolloverCandidates);

        if (matchedCsv.Count == 0)
        {
            ModelState.AddModelError(nameof(model.File), "No valid candidates found.");
            return View(model);
        }

        session.RolloverCandidates = matchedCsv;
        session.RolloverFundingStream = null;

        SaveSessionModel(session);

        return RedirectToAction("FundingStreamInclusionExclusion");
    }

    [HttpGet]
    [Route("/Review/Rollover/FundingStreamInclusionExclusion")]
    public async Task<IActionResult> FundingStreamInclusionExclusion()
    {
        var session = GetSessionModel();
        var model = new FundingStreamInclusionExclusionViewModel();

        if (session.RolloverFundingStream != null)
        {
            model.FundingStreams = session.RolloverFundingStream.FundingStreams;
            model.SelectedIds = session.RolloverFundingStream.SelectedIds;
        }
        else
        {
            model.FundingStreams = RolloverCandidateExtensions.ToFundingStreams(session.RolloverCandidates);

            if (model.FundingStreams.Count == 0)
            {
                ModelState.AddModelError(nameof(model.FundingStreams), "No Funding Streams found.");
                return View(model);
            }

            session.RolloverFundingStream = new RolloverFundingStream
            {
                FundingStreams = model.FundingStreams,
                SelectedIds = model.SelectedIds
            };

            SaveSessionModel(session);
        }

        return View(model);
    }

    [HttpPost]
    [Route("/Review/Rollover/FundingStreamInclusionExclusion")]
    public async Task<IActionResult> FundingStreamInclusionExclusion(FundingStreamInclusionExclusionViewModel vm, string action)
    {
        var session = GetSessionModel();
        var validIds = new List<Guid>();

        if (session.RolloverFundingStream != null)
        {
            validIds = session.RolloverFundingStream.FundingStreams.Select(x => x.Id).ToList();
            vm.FundingStreams = session.RolloverFundingStream.FundingStreams;
        }

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

        session.RolloverFundingStream = new RolloverFundingStream
        {
            FundingStreams = vm.FundingStreams,
            SelectedIds = vm.SelectedIds
        };

        SaveSessionModel(session);

        return RedirectToAction(nameof(EnterRolloverEligibilityDates));
    }

    [HttpGet]
    [Route("/Review/Rollover/EnterRolloverEligibilityDates")]
    public async Task<IActionResult> EnterRolloverEligibilityDates()
    {
        var session = GetSessionModel();
        var model = new RolloverEligibilityDatesViewModel();

        model.FundingEndDate = session.RolloverEligibilityDates?.FundingEndDate
                               ?? model.FundingEndDate;

        model.OperationalEndDate = session.RolloverEligibilityDates?.OperationalEndDate
                                   ?? model.OperationalEndDate;

        return View(model);
    }

    [HttpPost]
    [Route("/Review/Rollover/EnterRolloverEligibilityDates")]
    public async Task<IActionResult> EnterRolloverEligibilityDates(RolloverEligibilityDatesViewModel model)
    {
        var session = GetSessionModel();
        var validation = await _rolloverEligibilityDatesViewModeValidator.ValidateAsync(model);
        validation.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            return View("EnterRolloverEligibilityDates", model);
        }

        session.RolloverEligibilityDates = new RolloverEligibilityDates
        {
            FundingEndDate = model.FundingEndDate,
            OperationalEndDate = model.OperationalEndDate
        };

        SaveSessionModel(session);

        return RedirectToAction(nameof(EnterRolloverFundingApprovalEndDate));
    }

    [HttpGet]
    [Route("/Review/Rollover/EnterRolloverFundingApprovalEndDate")]
    public IActionResult EnterRolloverFundingApprovalEndDate()
    {
        var session = GetSessionModel();
        var model = new RolloverFundingApprovalEndDateViewModel();

        model.MaxApprovalEndDate = session.RolloverFundingApprovalEndDate
                               ?? model.MaxApprovalEndDate;

        return View(model);
    }

    [HttpPost]
    [Route("/Review/Rollover/EnterRolloverFundingApprovalEndDate")]
    public async Task<IActionResult> EnterRolloverFundingApprovalEndDate(RolloverFundingApprovalEndDateViewModel model)
    {
        var session = GetSessionModel();
        var validation = await _rolloverFundingApprovalEndDateViewModelViewModeValidator.ValidateAsync(model);

        validation.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            return View("EnterRolloverFundingApprovalEndDate", model);
        }

        session.RolloverFundingApprovalEndDate = new RolloverFundingApprovalEndDate
        {
            Day = model.MaxApprovalEndDate?.Day,
            Month = model.MaxApprovalEndDate?.Month,
            Year = model.MaxApprovalEndDate?.Year,
        };

        SaveSessionModel(session);

        var candidates = session.RolloverCandidates ?? new List<QualificationCandidate>();
        var eligibility = session.RolloverEligibilityDates;
        var stream = session.RolloverFundingStream;

        var academicYear = candidates
            .Select(c => c.AcademicYear)
            .FirstOrDefault(y => !string.IsNullOrWhiteSpace(y));

        var candidateIds = candidates
            .Select(c => c.RolloverCandidateId)
            .Distinct()
            .ToList();

        var fundingOfferIds = (stream?.SelectedIds ?? Enumerable.Empty<Guid>())
            .Distinct()
            .ToList();

        var command = new CreateRolloverWorkflowRunCommand()
        {
            AcademicYear = academicYear!,
            SelectionMethod = SelectionMethod.FileUpload,
            FundingEndDateEligibilityThreshold = session.RolloverEligibilityDates?.FundingEndDate?.ToDateTime(),
            OperationalEndDateEligibilityThreshold = session.RolloverEligibilityDates?.OperationalEndDate?.ToDateTime(),
            MaximumApprovalFundingEndDate = session.RolloverFundingApprovalEndDate?.ToDateTime(),
            RolloverCandidateIds = candidateIds,
            FundingOfferIds = fundingOfferIds,
            CreatedByUserName = _userHelperService.GetUserDisplayName()
        };

        var response = await Send(command);

        return RedirectToAction(nameof(InitialChecksExport));
    }

    [HttpGet]
    [Route("/Review/Rollover/InitialChecksExport")]
    public async Task<IActionResult> InitialChecksExport()
    {
        return View();
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