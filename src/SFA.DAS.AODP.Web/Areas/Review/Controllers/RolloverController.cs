using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Extensions;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Application.Queries.Rollover;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Infrastructure.Cache;
using AwardingOrganisation = SFA.DAS.AODP.Domain.Rollover.AwardingOrganisation;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;
using RolloverQueryBuilderRequestMapper = SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.RolloverQueryBuilderRequestMapper;
using RolloverSelectCandidates = SFA.DAS.AODP.Domain.Rollover.RolloverSelectCandidates;
using SectorSubjectAreaSelectionType = SFA.DAS.AODP.Domain.Rollover.SectorSubjectAreaSelectionType;

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
    public async Task<IActionResult> Index(RolloverStartViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("RolloverStart", model);
        }

        var session = GetSessionModel();
        (session.Start ??= new RolloverStart()).SetStart(session, model.SelectedProcess);
        SaveSessionModel(session);

        var candidateCount = await Send(new GetRolloverWorkflowCandidatesCountQuery());

        if (model.SelectedProcess is RolloverProcess.FinalUpload && candidateCount.TotalRecords is 0)
        {
            ModelState.AddModelError(nameof(model.SelectedProcess), "There is no existing rollover run in progress");
            return View("RolloverStart", model);
        }

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
        ModelState.Clear();
        return View(new RolloverUploadQualificationsViewModel { ReturnViewName = nameof(UploadQualificationsToRollover) });
    }

    [HttpPost]
    [Route("review/rollover/uploadqualificationstorollover")]
    public async Task<IActionResult> UploadQualificationsToRollover([FromForm] RolloverUploadQualificationsViewModel model)
    {
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
                        ProposedFundingApprovalEndDate = x.ProposedFundingApprovalEndDate.HasValue ? x.ProposedFundingApprovalEndDate.Value : default,
                        Comments = x.Comments
                    }
                    ).ToList()
            };

            var validationResponse = await Send(command);

            var session = GetSessionModel();
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
        model.ReturnViewName = nameof(RolloverValidationErrors);
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

            (session.ImportStatus ??= new RolloverImportStatus()).SetImportStatus(session, model.RegulatedQualificationsLastImported, model.FundedQualificationsLastImported, model.DefundingListLastImported, model.PldnsListLastImported);
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
                    (sessionModel.PreviousData ??= new RolloverPreviousData()).SetPreviousDataCandidate(sessionModel, previousData.CandidateCount, previousData.SelectedOption);
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

        (session.PreviousData ??= new RolloverPreviousData()).SetPreviousDataCandidate(session, model.CandidateCount, model.SelectedOption);
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
            (session.PreviousData ??= new RolloverPreviousData()).SetPreviousDataCandidate(session, model.CandidateCount, model.SelectedOption);
            SaveSessionModel(session);
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        if (model.SelectedOption == RolloverPreviousFileOption.RemovePrevious)
        {
            try
            {
                await Send(new RemovePreviousWorkflowCandidatesQuery());
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        return model.SelectedOption switch
        {
            RolloverPreviousFileOption.ContinueProcessing => RedirectToAction(nameof(FundingStreamInclusionExclusion)),
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
        (session.SelectCandidates ??= new RolloverSelectCandidates()).SetSelectCandidates(session, model.SelectedOption, model.ReturnUrl);
        SaveSessionModel(session);

        return model.SelectedOption switch
        {
            SelectCandidatesForRollover.ImportAList => RedirectToAction(nameof(UploadQualificationCandidates)),
            SelectCandidatesForRollover.GenerateAList  => RedirectToAction(nameof(SelectLevels)),
            _ => View()
        };
    }

    [HttpGet]
    [Route("review/rollover/uploadqualificationcandidates")]
    public async Task<IActionResult> UploadQualificationCandidates()
    {
        ModelState.Clear();
        return View(new RolloverUploadQualificationCandidatesViewModel());
    }

    [HttpPost]
    [Route("review/rollover/uploadqualificationcandidates")]
    public async Task<IActionResult> UploadQualificationCandidates([FromForm] RolloverUploadQualificationCandidatesViewModel model)
    {
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

        var session = GetSessionModel();
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
            model.SelectionMethod = session.SelectCandidates?.SelectedOption;
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
            // Should not have to do this as its horrible but its a quick fix for now. Need to rework the underlying Dates object to handle this more elegantly.
            ModelState.Where(o => o.Key.EndsWith(".Day")).Select(o => o.Value).ToList().ForEach(o => o.ValidationState = ModelValidationState.Skipped);
            ModelState.Where(o => o.Key.EndsWith(".Year")).Select(o => o.Value).ToList().ForEach(o => o.ValidationState = ModelValidationState.Skipped);
            ModelState.Where(o => o.Key.EndsWith(".Month")).Select(o => o.Value).ToList().ForEach(o => o.ValidationState = ModelValidationState.Skipped);

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

        var fundingOfferIds = stream?
            .SelectedIds
            .Distinct()
            .ToList() ?? [];

        var academicYear = candidates
            .Select(c => c.AcademicYear)
            .FirstOrDefault(y => !string.IsNullOrWhiteSpace(y));

        //Only send candidates if their offer type is selected
        var candidateIds = candidates
            .Where(c => fundingOfferIds.Contains(c.FundingOfferId))
            .Select(c => c.RolloverCandidateId)
            .Distinct()
            .ToList();

        var command = new CreateRolloverWorkflowRunCommand()
        {
            AcademicYear = academicYear!,
            SelectionMethod = session.SelectCandidates?.SelectedOption is SelectCandidatesForRollover.GenerateAList ? SelectionMethod.QueryBuilder : SelectionMethod.FileUpload,
            FundingEndDateEligibilityThreshold = session.RolloverEligibilityDates?.FundingEndDate?.ToDateTime(),
            OperationalEndDateEligibilityThreshold = session.RolloverEligibilityDates?.OperationalEndDate?.ToDateTime(),
            MaximumApprovalFundingEndDate = model.MaxApprovalEndDate?.ToDateTime(),
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
    [Route("api/rollover/GetRolloverCandidatesForExport")]
    public async Task<IActionResult> GetRolloverCandidatesForExport([FromQuery]Guid rolloverWorkflowRunId)
    {
        var response = await Send(new GetRolloverCandidatesForExportQuery { RolloverWorkflowRunId = rolloverWorkflowRunId });

        return File(response.FileContent, response.ContentType, response.FileName);
    }

    [HttpGet]
    [Route("review/rollover/InitialChecksExport")]
    public IActionResult InitialChecksExport()
    {
        var session = GetSessionModel();
        session.QueryBuilderFilters = new QueryBuilderFilters();

        SaveSessionModel(session);

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

    [HttpPost]
    [Route("/Review/Rollover/SelectLevels")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectLevels(SelectQualificationLevelsViewModel model, string action)
    {
        var response = await Send(new GetLevelsForRolloverQueryBuilderQuery());
        model.SetLevels(response.Levels);

        if (!ModelState.IsValid && action != "selectAll")
        {
            return View(model);
        }

        if (action == "selectAll")
        {
            model.MarkAllAsChecked();
            ModelState.Clear();
            return View(model);
        }

        var session = GetSessionModel();

        session.QueryBuilderFilters = session.QueryBuilderFilters.SetLevels(model.SelectedLevels);

        SaveSessionModel(session);

        return RedirectToAction(nameof(SelectTypes));
    }


    [HttpGet]
    [Route("/Review/Rollover/SelectLevels")]
    public async Task<IActionResult> SelectLevels()
    {
        var model = new SelectQualificationLevelsViewModel();
        var session = GetSessionModel();

        if (session.QueryBuilderFilters.Levels.Count > 0)
        {
            model.SelectedLevels = session.QueryBuilderFilters.Levels.ToList();
        }

        var response = await Send(new GetLevelsForRolloverQueryBuilderQuery());
        model.SetLevels(response.Levels);

        return View(model);
    }

    [HttpGet]
    [Route("/Review/Rollover/SelectTypes")]
    public async Task<IActionResult> SelectTypes()
    {
        var model = new SelectQualificationTypesViewModel();
        var session = GetSessionModel();

        if (session.QueryBuilderFilters.Types.Count > 0)
        {
            model.SelectedTypes = session.QueryBuilderFilters.Types.ToList();
        }

        var response = await Send(new GetTypesForRolloverQueryBuilderQuery(RolloverQueryBuilderRequestMapper.ForTypesFilter(session.QueryBuilderFilters)));
        model.Set(response.Types);

        return View(model);
    }

    [HttpPost]
    [Route("/Review/Rollover/SelectTypes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectTypes(SelectQualificationTypesViewModel model, string action)
    {
        var session = GetSessionModel();

        var response = await Send(new GetTypesForRolloverQueryBuilderQuery(RolloverQueryBuilderRequestMapper.ForTypesFilter(session.QueryBuilderFilters)));
        model.Set(response.Types);

        if (!ModelState.IsValid && action != "selectAll")
        {
            return View(model);
        }

        if (action == "selectAll")
        {
            model.MarkAllAsChecked();
            ModelState.Clear();
            return View(model);
        }

        session.QueryBuilderFilters = session.QueryBuilderFilters.SetTypes(model.SelectedTypes);

        SaveSessionModel(session);

        return RedirectToAction(nameof(SelectSectorSubjectArea));
    }

    [HttpGet]
    [Route("/Review/Rollover/SelectSectorSubjectArea")]
    public async Task<IActionResult> SelectSectorSubjectArea()
    {
        var session = GetSessionModel();
        var model = new SelectSectorSubjectAreasModel();

        if (session.QueryBuilderFilters.SectorSubjectAreas.Count > 0)
        {
            model.SelectedSectorSubjectAreas = session.QueryBuilderFilters.SectorSubjectAreas.ToList();
            model.SelectionType = session.QueryBuilderFilters.SectorSubjectAreasSelectionType;
        }

        var response = await Send(new GetSectorSubjectAreaForRolloverQueryBuilderQuery(RolloverQueryBuilderRequestMapper.ForSectorSubjectAreaFilter(session.QueryBuilderFilters)));
        model.Set(response.SectorSubjectAreas);
        
        model.SelectionType = session.QueryBuilderFilters.SectorSubjectAreasSelectionType;

        return View(model);
    }

    [HttpPost]
    [Route("/Review/Rollover/SelectSectorSubjectArea")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectSectorSubjectArea(SelectSectorSubjectAreasModel model, string action)
    {
        var session = GetSessionModel();

        var response = await Send(new GetSectorSubjectAreaForRolloverQueryBuilderQuery(RolloverQueryBuilderRequestMapper.ForSectorSubjectAreaFilter(session.QueryBuilderFilters)));
        model.Set(response.SectorSubjectAreas);

        if (model.SelectionType is SectorSubjectAreaSelectionType.None)
        {
            ModelState.Remove(nameof(model.SelectedSectorSubjectAreas));

            ModelState.AddModelError(nameof(model.SelectionType), "Select if you want to rollover all SSAs or only a selection");

            return View(model);
        }

        if (model.SelectionType is SectorSubjectAreaSelectionType.All)
        {
            model.MarkAllAsChecked();
        }

        if (model.SelectionType is SectorSubjectAreaSelectionType.SpecificSelection)
        {
            if (action == "selectAll")
            {
                model.MarkAllAsChecked(true);
                ModelState.Clear();
                return View(model);
            }

            if (model.SelectedSectorSubjectAreas.Count <= 0)
            {
                ModelState.AddModelError($"{nameof(model.SelectedSectorSubjectAreas)}", "You must select at least one SSA");
                return View(model);
            }
        }

        session.QueryBuilderFilters = session.QueryBuilderFilters.SetSectorSubjectAreas(model.SelectedSectorSubjectAreas, response.SectorSubjectAreas, model.SelectionType);

        SaveSessionModel(session);

        return RedirectToAction(nameof(SelectAwardingOrganisations));
    }

    [HttpGet]
    [Route("/Review/Rollover/SelectAwardingOrganisations")]
    public async Task<IActionResult> SelectAwardingOrganisations()
    {
        var session = GetSessionModel();
        var model = new SelectAwardingOrganisationsViewModel();

        model.SelectionType = session.QueryBuilderFilters.AwardingOrganisationSelectionType;

        if (session.QueryBuilderFilters.SelectedAwardingOrganisationIds.Count > 0)
        {
            model.SelectedAwardingOrganisations = session.QueryBuilderFilters.SelectedAwardingOrganisationIds.Distinct().ToList();
        }
        
        model = await BuildSelectAwardingOrganisationsViewModel();
        
        return View(model);
    }

    [HttpPost]
    [Route("/Review/Rollover/SelectAwardingOrganisations")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectAwardingOrganisations(SelectAwardingOrganisationsViewModel model, string action)
    {
        var awardingOrganisations = await GetAwardingOrganisationsForSelectedRolloverFilters();
        model.AwardingOrganisations = SelectAwardingOrganisationsViewModel.Create(
            awardingOrganisations,
            model.SelectedAwardingOrganisations,
            model.SelectionType).AwardingOrganisations;

        if (model.SelectionType is AwardingOrganisationSelectionType.None)
        {
            ModelState.Remove(nameof(model.SelectedAwardingOrganisations));

            ModelState.AddModelError(nameof(model.SelectionType), "Select if you want to rollover all awarding organisations or only a selection");

            return View(model);
        }

        if (model.SelectionType is AwardingOrganisationSelectionType.All)
        {
            model.MarkAllAsChecked();
        }

        if (model.SelectionType is AwardingOrganisationSelectionType.SpecificSelection)
        {
            if (action == "selectAll")
            {
                model.MarkAllAsChecked(true);
                ModelState.Clear();
                return View(model);
            }

            if (model.SelectedAwardingOrganisations.Count <= 0)
            {
                ModelState.AddModelError($"{nameof(model.SelectedAwardingOrganisations)}", "You must select at least one awarding organisation");
                
                return View(model);
            }
        }

        var session = GetSessionModel();

        session.QueryBuilderFilters = session.QueryBuilderFilters.SetAwardingOrganisations(model.SelectedAwardingOrganisations, awardingOrganisations, model.SelectionType);

        SaveSessionModel(session);

        return RedirectToAction(nameof(CheckYourAnswers));
    }

    [HttpGet]
    [Route("/Review/Rollover/CheckYourAnswers")]
    public async Task<IActionResult> CheckYourAnswers()
    {
        var session = GetSessionModel();

        if (!session.QueryBuilderFilters.CanProgress(out var missingFilter))
        {
            if (!string.IsNullOrEmpty(missingFilter))
            {
                return missingFilter switch
                {
                    nameof(QueryBuilderFilters.Levels) => RedirectToAction(nameof(SelectLevels)),
                    nameof(QueryBuilderFilters.Types) => RedirectToAction(nameof(SelectTypes)),
                    nameof(QueryBuilderFilters.SectorSubjectAreas) => RedirectToAction(nameof(SelectSectorSubjectArea)),
                    nameof(QueryBuilderFilters.AwardingOrganisationSelectionType) => RedirectToAction(nameof(SelectAwardingOrganisations)),
                    _ => RedirectToAction(nameof(SelectCandidates))
                };
            }
        }

        var qualificationVersions = await Send(
            new GetQualificationVersionsForRolloverQueryBuilderQuery(RolloverQueryBuilderRequestMapper.ForAll(session.QueryBuilderFilters)));
        
        var model = new CheckYourAnswersViewModel
        {
            Levels = session.QueryBuilderFilters.Levels.ToList(),
            Types = session.QueryBuilderFilters.Types.ToList(),
            SectorSubjectAreas = session.QueryBuilderFilters.SectorSubjectAreas.ToList(),
            SectorSubjectAreaSelectionType= session.QueryBuilderFilters.SectorSubjectAreasSelectionType,
            AllAwardingOrganisationsCount = session.QueryBuilderFilters.AllAwardingOrganisations.Count,
            AwardingOrganisations = session.QueryBuilderFilters.SelectedAwardingOrganisations.ToList(),
            AwardingOrganisationSelectionType = session.QueryBuilderFilters.AwardingOrganisationSelectionType,
            ExcludedSectorSubjectAreas = session.QueryBuilderFilters.ExcludedSectorSubjectAreas,
            ExcludedAwardingOrganisations = session.QueryBuilderFilters.ExcludedAwardingOrganisations,
            CandidateCount = qualificationVersions?.QualificationVersions.Count() ?? 0,
        };

        return View(model);
    }
    
    [HttpPost]
    [Route("/Review/Rollover/CheckYourAnswers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckYourAnswers(CheckYourAnswersViewModel model)
    {
        var session = GetSessionModel();
        var response = await Send(
            new GetQualificationVersionsForRolloverQueryBuilderQuery(RolloverQueryBuilderRequestMapper.ForAll(session.QueryBuilderFilters)));

        if (!response.QualificationVersions.Any())
        {
            _logger.LogWarning("No candidates found, redirect back to beginning of query builder journey.");
            
            return RedirectToAction(nameof(SelectLevels));
        }

        session.RolloverCandidates = response.QualificationVersions.Select(o => new QualificationCandidate
        {
            RolloverCandidateId = o.Id,
            AcademicYear = o.AcademicYear,
            QualificationVersionId = o.QualificationVersionId,
            QualificationNumber = o.QualificationNumber,
            FundingOfferId = o.FundingOfferId,
            FundingOfferName = o.FundingOfferName,
            FundingApprovalEndDate = o.PreviousFundingEndDate,
            QualificationName = o.QualificationName
        }).ToList();

        SaveSessionModel(session);

        return RedirectToAction(nameof(FundingStreamInclusionExclusion));
    }

    private async Task<SelectAwardingOrganisationsViewModel> BuildSelectAwardingOrganisationsViewModel()
    {
        var session = GetSessionModel();
        var awardingOrganisations = await GetAwardingOrganisationsForSelectedRolloverFilters();

        return SelectAwardingOrganisationsViewModel.Create(
            awardingOrganisations,
            session.QueryBuilderFilters.SelectedAwardingOrganisationIds,
            session.QueryBuilderFilters.AwardingOrganisationSelectionType);
    }

    private async Task<List<AwardingOrganisation>> GetAwardingOrganisationsForSelectedRolloverFilters()
    {
        var session = GetSessionModel();
        var response = await Send(new GetAwardingOrganisationsForRolloverQueryBuilderQuery(
            RolloverQueryBuilderRequestMapper.ForAwardingOrganisationFilter(session.QueryBuilderFilters)));

        return response?.AwardingOrganisations.ToList() ?? [];
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