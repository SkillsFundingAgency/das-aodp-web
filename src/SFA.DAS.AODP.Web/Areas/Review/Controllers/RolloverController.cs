using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Matching;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Application.Queries.Rollover;
using SFA.DAS.AODP.Infrastructure.Cache;
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
    private readonly ICsvFileReader _csvFileReader;
    private readonly IValidator<RolloverEligibilityDatesViewModel> _rolloverEligibilityDatesViewModeValidator;
    private readonly IValidator<RolloverFundingApprovalEndDateViewModel> _rolloverFundingApprovalEndDateViewModelViewModeValidator;
    private readonly IUserHelperService _userHelperService;
    private readonly ICacheService _cacheService;

    public RolloverController(ILogger<RolloverController> logger,
        IMediator mediator,
        IValidator<RolloverEligibilityDatesViewModel> validatorEligibilityDates,
        IValidator<RolloverFundingApprovalEndDateViewModel> validatorApprovalEndDate,
        ICsvFileReader csvFileReader,
        IUserHelperService userHelperService,
        ICacheService cacheService) : base(mediator, logger)
    {
        _logger = logger;
        _rolloverEligibilityDatesViewModeValidator = validatorEligibilityDates;
        _rolloverFundingApprovalEndDateViewModelViewModeValidator = validatorApprovalEndDate;
        _csvFileReader = csvFileReader;
        _cacheService = cacheService;
        _userHelperService = userHelperService;
    }

    [HttpGet]
    [Route("review/rollover")]
    public IActionResult Index()
    {
        var session = GetSessionModel();
        var model = session.Start != null
            ? new RolloverStartViewModel { SelectedProcess = session.Start.SelectedProcess }
            : new RolloverStartViewModel();

        return View("RolloverStart", model);
    }

    [HttpPost]
    [Route("review/rollover")]
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
            RolloverProcess.FinalUpload => RedirectToAction(nameof(UploadQualificationsToRollover)),
            _ => View("RolloverStart", model)
        };
    }

    [HttpGet]
    [Route("review/rollover/initialselection")]
    public IActionResult InitialSelection()
    {
        ViewData["Title"] = "Initial selection of qualificaton";
        return View();
    }

    [HttpGet]
    [Route("review/rollover/uploadqualificationstorollover")]
    public IActionResult UploadQualificationsToRollover()
    {
        return View(new RolloverUploadQualificationsViewModel());
    }

    [HttpPost]
    [Route("review/rollover/uploadqualificationstorollover")]
    public async Task<IActionResult> UploadQualificationsToRollover([FromForm] RolloverUploadQualificationsViewModel model)
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

        try
        {
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

            var command = new ValidateRolloverExtensionCommand
            {
                RolloverCandidates = file.Items.Select(
                    x => new RolloverCandidateForValidation 
                    { 
                        Qan = x.Qan,
                        FundingStreamName = x.FundingStreamName ?? string.Empty,
                        RollOverStatus = x.RollOverStatus ?? string.Empty,
                        ExclusionReason = x.ExclusionReason,
                        ProposedFundingApprovalEndDate = x.ProposedFundingApprovalEndDate,
                        Comments = x.Comments
                    }
                    ).ToList()
            };

            var validationResponse = await Send(command);

            session.RolloverFundingExtensionCandidates = file.Items;
            SaveSessionModel(session);

            if (!validationResponse.IsValid)
            {
                var token = Guid.NewGuid().ToString("N");

                await _cacheService.SetAsync(
                    $"download:validation:{token}",
                    validationResponse.ValidationFailureSummary?.ValidatedCandidateFile ?? Array.Empty<byte>()
                );

                var notValidForRollover = validationResponse.ValidationFailureSummary?.NotIncludedInRollover?
                    .Select(c => new RolloverValidationErrorItem
                    {
                        Qan = c.Qan,
                        FundingStream = c.FundingStream,
                        ErrorMessages = c.Errors
                    })
                    .ToList() ?? new List<RolloverValidationErrorItem>();

                model.ValidationSummary = new RolloverValidationErrorViewModel
                {
                    ErrorFileToken = token,
                    FailedCandidateCount = validationResponse.ValidationFailureSummary?.FailedCandidateCount ?? 0,
                    NotValidCandidates = notValidForRollover
                };

                return View(nameof(RolloverValidationErrors), model);
            }
            else
            {
                var summaryModel = new RolloverSummaryViewModel(validationResponse.ValidationSuccessSummary!);
                return RedirectToAction(nameof(RolloverSummary), summaryModel);

            }
        }
        catch (Exception ex)
        {
            LogException(ex);
            ModelState.AddModelError("", "An unexpected error occurred while validating the file.");
            return View(model);
        }   
    }

    [HttpGet]
    [Route("review/rollover/downloadcandidatevalidationerrors")]
    public async Task<IActionResult> DownloadCandidateValidationErrors(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest();
        }

        var cacheKey = $"download:validation:{token}";

        var fileBytes = await _cacheService.GetAsync<byte[]>(cacheKey);

        if (fileBytes == null || fileBytes.Length == 0)
        {
            return NotFound();
        }

        await _cacheService.RemoveAsync(cacheKey);

        return File(
            fileBytes,
            "text/csv",
            "validation-errors.csv"
        );
    }

    [HttpGet]
    [Route("review/rollover/rolloversummary")]
    public IActionResult RolloverSummary(RolloverSummaryViewModel model)
    {
        return View(model);
    }

    [HttpPost]
    [Route("review/rollover/rolloversummary")]
    public async Task<IActionResult> RolloverSummary()
    {
        var session = GetSessionModel();

        if (session.RolloverFundingExtensionCandidates == null ||
            session.RolloverFundingExtensionCandidates.Count == 0)
        {
            return RedirectToAction(nameof(UploadQualificationsToRollover));
        }

        var command = new SubmitRolloverExtensionCommand
        {
            Items = session.RolloverFundingExtensionCandidates.Select(
                x => new FundingExtensionItem 
                { 
                    Qan =  x.Qan!,
                    FundingStreamName = x.FundingStreamName!,
                    RolloverStatus = x.RollOverStatus!,
                    ExclusionReason = x.ExclusionReason,
                    ProposedFundingApprovalEndDate = x.ProposedFundingApprovalEndDate!.Value,
                    Comments = x.Comments
                }).ToList()
        };

        await Send(command);

        ClearSessionModel();

        return RedirectToAction(nameof(RolloverSubmitted));
    }

    [HttpGet]
    [Route("Review/Rollover/RolloverSubmitted")]
    public async Task<IActionResult> RolloverSubmitted()
    {
        return View();
    }


    [HttpGet]
    [Route("review/rollover/rollovervalidationerrors")]
    public async Task<IActionResult> RolloverValidationErrors(RolloverUploadQualificationsViewModel model)
    {
        return View(model);
    }

    [HttpGet]
    [Route("review/rollover/checkdata")]
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
    [Route("review/rollover/checkdata")]
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
    [Route("review/rollover/previousfile")]
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
    [Route("review/rollover/previousfile")]
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
    [Route("review/rollover/selectcandidates")]
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
    [Route("review/rollover/selectcandidates")]
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
    [Route("review/rollover/rolloverqerybuilder")]
    public IActionResult RolloverQueryBuilder()
    {
        ViewData["Title"] = "Rollover Query Builder";
        return View();
    }

    [HttpGet]
    [Route("review/rollover/selectfundingstreams")]
    public IActionResult SelectFundingStreams()
    {
        ViewData["Title"] = "Select funding stream(s)";
        return View();
    }

    [HttpGet]
    [Route("review/rollover/uploadqualificationcandidates")]
    public async Task<IActionResult> UploadQualificationCandidates()
    {
        return View(new RolloverUploadQualificationCandidatesViewModel());
    }

    [HttpPost]
    [Route("review/rollover/uploadqualificationcandidates")]
    public async Task<IActionResult> UploadQualificationCandidates([FromForm] RolloverUploadQualificationCandidatesViewModel model)
    {
        var session = GetSessionModel();

        if (model.File == null && session.RolloverCandidates.Count > 0)
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
    [Route("review/rollover/fundingstreaminclusionexclusion")]
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
    [Route("review/rollover/fundingstreaminclusionexclusion")]
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
    [Route("review/rollover/enterrollovereligibilitydates")]
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
    [Route("review/rollover/enterrollovereligibilitydates")]
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
    [Route("review/rollover/enterrolloverfundingapprovalenddate")]
    public IActionResult EnterRolloverFundingApprovalEndDate()
    {
        var session = GetSessionModel();
        var model = new RolloverFundingApprovalEndDateViewModel();

        model.MaxApprovalEndDate = session.RolloverFundingApprovalEndDate
                               ?? model.MaxApprovalEndDate;

        return View(model);
    }

    [HttpPost]
    [Route("review/rollover/enterrolloverfundingapprovalenddate")]
    public async Task<IActionResult> EnterRolloverFundingApprovalEndDate(RolloverFundingApprovalEndDateViewModel model)
    {
        var session = GetSessionModel();
        var validation = await _rolloverFundingApprovalEndDateViewModelViewModeValidator.ValidateAsync(model);

        validation.AddToModelState(ModelState);

        if (!ModelState.IsValid)
        {
            return View("EnterRolloverFundingApprovalEndDate", model);
        }

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

        session.RolloverFundingApprovalEndDate = new RolloverFundingApprovalEndDate
        {
            Day = model.MaxApprovalEndDate?.Day,
            Month = model.MaxApprovalEndDate?.Month,
            Year = model.MaxApprovalEndDate?.Year,
        };
        TempData["RolloverWorkflowRunId"] = response.RolloverWorkflowRunId;
        session.WorkflowRunId = response.RolloverWorkflowRunId;
        SaveSessionModel(session);

        return RedirectToAction(nameof(InitialChecksExport));

    }

    [HttpGet]
    [Route("review/rollover/GetRolloverCandidatesForExport")]
    public async Task<IActionResult> GetRolloverCandidatesForExport([FromQuery]Guid rolloverWorkflowRunId)
    {
        var response = await Send(new GetRolloverCandidatesForExportQuery { RolloverWorkflowRunId = rolloverWorkflowRunId });

        return File(response.FileContent, response.ContentType, response.FileName);
    }



    [HttpGet]
    [Route("review/rollover/InitialChecksExport")]
    public IActionResult InitialChecksExport()
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

    private void ClearSessionModel()
    {
        try
        {
            HttpContext.Session.Remove(SessionKey);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

}