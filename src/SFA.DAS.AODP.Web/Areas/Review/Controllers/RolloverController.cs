using DocumentFormat.OpenXml.EMMA;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Application.Queries.Review.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Domain.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Extensions;
using SFA.DAS.AODP.Web.Areas.Review.Helpers.Rollover;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Extensions;

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

    public RolloverController(ILogger<RolloverController> logger, IMediator mediator, IValidator<RolloverEligibilityDatesViewModel> validator, ICsvFileReader csvFileReader) : base(mediator, logger)
    {
        _logger = logger;
        _rolloverEligibilityDatesViewModeValidator = validator;
        _csvFileReader = csvFileReader;
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
        session.Start = new RolloverStart
        {
            SelectedProcess = model.SelectedProcess
        };

        SaveSessionModel(session);

        return model.SelectedProcess switch
        {
            RolloverProcess.InitialSelection => RedirectToAction(nameof(CheckData)),
            RolloverProcess.FinalUpload => RedirectToAction(nameof(UploadQualifications)),
            _ => View(RolloverStartView, model)
        };
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

            session.ImportStatus = RolloverImportStatusViewModel.MapToSession(model);

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
            var candidateCount = await Send(new GetRolloverWorkflowCandidatesQuery());
            count = candidateCount?.Data?.Count ?? 0;

            if (count > 0)
            {
                try
                {
                    sessionModel.PreviousData = new RolloverPreviousData
                    {
                        CandidateCount = count
                    };
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

        var candidateCount = await Send(new GetRolloverWorkflowCandidatesQuery());

        var model = new RolloverPreviousDataViewModel
        {
            CandidateCount = candidateCount.Data.Count
        };

        session.PreviousData = new RolloverPreviousData
        {
            CandidateCount = model.CandidateCount
        };
        SaveSessionModel(session);

        ViewData["Title"] = "We found a list of candidates for rollover you worked on previously.";
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
            session.PreviousData!.SelectedOption = model.SelectedOption;
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
    public IActionResult SelectCandidates(string? returnAction = null)
    {
        ViewData["Title"] = "How do you want to select candidates for rollover";
        ViewData["ReturnAction"] = returnAction ?? nameof(CheckData);
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
        var validIds = new List<string>();

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
    public async Task<IActionResult> EnterRolloverEligibilityDates() => View();

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