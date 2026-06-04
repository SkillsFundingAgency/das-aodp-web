using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Qualification;
using SFA.DAS.AODP.Application.Commands.Qualifications;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Services;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Mappers;
using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Globalization;
using System.Text.Json;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    [Route("{controller}/{action}")]
    [Authorize(Policy = PolicyConstants.IsReviewUser)]
    public class ChangedController : ControllerBase
    {
        private readonly ILogger<ChangedController> _logger;
        private readonly IMediator _mediator;
        private readonly IUserHelperService _userHelperService;
        private readonly IQualificationTimelineHistoryBuilder _qualificationTimelineHistoryBuilder;

        private List<string> ReviewerAllowedStatuses { get; set; } =
        [
            ProcessStatus.DecisionRequired,
            ProcessStatus.NoActionRequired,
        ];

        private List<string> BulkUpdateAllowedStatuses { get; set; } = new List<string>()
        {
            ProcessStatus.DecisionRequired,
            ProcessStatus.NoActionRequired,
            ProcessStatus.OnHold
        };

        public enum NewQualDataKeys { InvalidPageParams, CommentSaved}

        public ChangedController(
                ILogger<ChangedController> logger, 
                IMediator mediator, 
                IUserHelperService userHelperService, 
                IQualificationTimelineHistoryBuilder  qualificationTimelineHistoryBuilder
            ) : base(mediator, logger)
        {
            _logger = logger;
            _mediator = mediator;
            this._userHelperService = userHelperService;
            _qualificationTimelineHistoryBuilder = qualificationTimelineHistoryBuilder;
        }

        public async Task<IActionResult> Index(QualificationQuery qualificationQuery, bool selectAll = false)
        {
            try { 
                ValidatePagingAndNotify(qualificationQuery);

                var viewModel = await BuildIndexViewModelAsync(qualificationQuery, selectAll);

                ShowNotificationIfKeyExists(
                    BulkActionQualifications.SuccessKey,
                    ViewNotificationMessageType.Success,
                    BulkActionQualifications.SuccessMessage);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Redirect("/Home/Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search(ChangedQualificationsViewModel viewModel)
        {
            try
            {
                return RedirectToAction(nameof(Index), new
                {
                    pageNumber = 1,
                    recordsPerPage = viewModel.PaginationViewModel.RecordsPerPage,
                    name = viewModel.Filter.QualificationName,
                    organisation = viewModel.Filter.Organisation,
                    qan = viewModel.Filter.QAN,
                    processStatusIds = viewModel.Filter.ProcessStatusIds,
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
                return View("Index", viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Clear(int recordsPerPage = 10)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Index), new
                    {
                        pageNumber = 0,
                        recordsPerPage = recordsPerPage,
                    });
                }
                else
                {
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return View("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePage(
            QualificationQuery qualificationQuery, 
            int newPage = 1)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return RedirectToAction(
                        nameof(Index), 
                        qualificationQuery.ToRouteValues(pageNumberOverride:newPage));
                }
                else
                {
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return View("Index");
            }
        }

        [Route("/Review/Changed/QualificationDetails/Timeline")]
        public async Task<IActionResult> QualificationDetailsTimeline([FromQuery] string qualificationReference)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                return Redirect("/Home/Error");
            }

            try
            {
                var response = await Send(new GetQualificationTimelineQuery
                {
                    QualificationReference = qualificationReference
                });

                var viewModel = (QualificationDetailsTimelineViewModel)response;
                viewModel.Qan = qualificationReference;

                return View(viewModel); 
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Redirect("/Home/Error");
            }
        }

        [HttpPost]
        [Route("/Review/Changed/ApplyBulkAction")]
        public async Task<IActionResult> ApplyBulkAction(
            ChangedQualificationsViewModel model,
            QualificationQuery qualificationQuery)
        {
            if (!ModelState.IsValid)
            {
                ValidatePagingAndNotify(qualificationQuery);

                var viewModel = await BuildIndexViewModelAsync(
                    qualificationQuery,
                    postedModel: model); 

                return View("Index", viewModel);
            }

            try
            {
                var result = await Send(new BulkUpdateQualificationStatusCommand
                {
                    QualificationIds = model.SelectedQualificationIds,
                    ProcessStatusId = model.BulkAction.ProcessStatusId!.Value,
                    Comment = model.BulkAction.Comment,
                    UserDisplayName = HttpContext.User?.Identity?.Name
                });

                if (result.ErrorCount == 0)
                {
                    TempData[BulkActionQualifications.SuccessKey] = true;

                    return RedirectToAction(nameof(Index), qualificationQuery.ToRouteValues());
                }

                var failed = result.Errors.Select(e => new QualificationBulkActionErrorItemViewModel
                {
                    QualificationId = e.QualificationId,
                    Qan = e.Qan ?? "",
                    Title = e.Title ?? "",
                    FailureReason = e.ErrorType switch
                    {
                        BulkQualificationErrorType.Missing => "Qualification not found.",
                        BulkQualificationErrorType.StatusUpdateFailed => "Status update failed.",
                        BulkQualificationErrorType.HistoryFailed => "Status updated but history was not updated.",
                        _ => "Unknown error."
                    }
                }).ToList();

                var errorModel = new QualificationBulkActionErrorModel
                {
                    Failed = failed,
                    BackLinkText = "Go back to Changed qualifications",
                    BackLinkUrl = Url.Action(nameof(Index), qualificationQuery.ToRouteValues())!
                };

                TempData[BulkActionQualifications.Errors] = JsonSerializer.Serialize(errorModel);
                return RedirectToAction(nameof(BulkQualificationError));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Redirect("/Home/Error");
            }
        }

        [HttpGet]
        public IActionResult BulkQualificationError()
        {
            var json = TempData[BulkActionQualifications.Errors] as string;

            var model = !string.IsNullOrEmpty(json)
                ? JsonSerializer.Deserialize<QualificationBulkActionErrorModel>(json)
                    ?? new QualificationBulkActionErrorModel()
                : new QualificationBulkActionErrorModel();

            return View(model);
        }

        

        [Route("/Review/Changed/QualificationDetails")]
        public async Task<IActionResult> QualificationDetails([FromQuery] string qualificationReference)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                return Redirect("/Home/Error");
            }
            try
            {
                ChangedQualificationDetailsViewModel latestVersion = await Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference });
                latestVersion.ProcessStatuses = [.. await GetProcessStatuses()];

                ShowNotificationIfKeyExists(NewQualDataKeys.CommentSaved.ToString(), ViewNotificationMessageType.Success, "The comment has been saved.");

                var feedbackForQualificationFunding = await Send(new GetFeedbackForQualificationFundingByIdQuery(latestVersion.Id));
                if (feedbackForQualificationFunding != null)
                {
                    latestVersion.MapFundedOffers(feedbackForQualificationFunding);
                    latestVersion.FundingsOffersOutcomeStatus = feedbackForQualificationFunding.Approved;
                }

                var applications = await Send(new GetApplicationsByQanQuery(latestVersion.Qual.Qan));
                if (applications != null)
                    latestVersion.Applications = ApplicationMapper.Map(applications);

                if (latestVersion.Version > 1)
                {
                    var previousVersion = await Send(new GetQualificationVersionQuery() { QualificationReference = qualificationReference, Version = latestVersion.Version - 1 });
                    var currentVersionForComparison = await Send(new GetQualificationVersionQuery() { QualificationReference = qualificationReference, Version = latestVersion.Version });

                    latestVersion.KeyFieldChanges = _qualificationTimelineHistoryBuilder.GetKeyFieldChanges(
                        previousVersion, currentVersionForComparison);
                }
                return View(latestVersion);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Redirect("/Home/Error");
            }
        }

        [Route("/Review/Changed/QualificationDetails")]
        [HttpPost]
        public async Task<IActionResult> QualificationDetails(ChangedQualificationDetailsViewModel model)
        {
            try
            {
                Guid? procStatus = model.AdditionalActions.ProcessStatusId;
                if (!procStatus.HasValue && !string.IsNullOrEmpty(model.AdditionalActions.Note))
                {
                    await Send(new AddQualificationDiscussionHistoryCommand
                    {
                        QualificationReference = model.Qual.Qan,
                        Notes = model.AdditionalActions.Note,
                        UserDisplayName = HttpContext.User?.Identity?.Name
                    });

                    TempData[NewQualDataKeys.CommentSaved.ToString()] = true;
                    return RedirectToAction(nameof(QualificationDetails), new { qualificationReference = model.Qual.Qan });
                }
                else if (!procStatus.HasValue)
                    return RedirectToAction(nameof(QualificationDetails), new { qualificationReference = model.Qual.Qan });

                model.ProcessStatuses = [.. await GetProcessStatuses()];
                if (!CheckUserIsAbleToSetStatus(model, procStatus.Value))
                    return RedirectToAction(nameof(QualificationDetails), new { qualificationReference = model.Qual.Qan });

                await Send(new UpdateQualificationStatusCommand
                {
                    QualificationReference = model.Qual.Qan,
                    ProcessStatusId = procStatus.Value,
                    Notes = model.AdditionalActions.Note,
                    Version = model.Version,
                    UserDisplayName = HttpContext.User?.Identity?.Name
                });
                return RedirectToAction(nameof(QualificationDetails), new { qualificationReference = model.Qual.Qan });
            }
            catch (Exception ex)
            {
                LogException(ex);
                return RedirectToAction(nameof(QualificationDetails), new { qualificationReference = model.Qual.Qan });
            }
        }

        private bool CheckUserIsAbleToSetStatus(ChangedQualificationDetailsViewModel model, Guid procStatusId)
        {
            if (_userHelperService.GetUserRoles().Contains(RoleConstants.QFAUApprover))
                return true;
            string processStatName = model.ProcessStatuses.FirstOrDefault(v => v.Id == procStatusId)?.Name ?? "";
            return ReviewerAllowedStatuses.Contains(processStatName);
        }

        public async Task<List<GetProcessStatusesQueryResponse.ProcessStatus>> GetProcessStatuses()
        {
            var procStatuses = await Send(new GetProcessStatusesQuery());
            if (!_userHelperService.GetUserRoles().Contains(RoleConstants.QFAUApprover))
            {
                return procStatuses.ProcessStatuses
                    .Where(p => ReviewerAllowedStatuses.Contains(p.Name ?? "")).ToList();
            }
            return procStatuses.ProcessStatuses;
        }

        private async Task<ChangedQualificationsViewModel> BuildIndexViewModelAsync(
            QualificationQuery qualificationQuery,
            bool selectAll = false,
            ChangedQualificationsViewModel? postedModel = null)
        {
            var procStatuses = await Send(new GetProcessStatusesQuery());
            var statuses = procStatuses.ProcessStatuses ?? new();

            ChangedQualificationsViewModel vm;

            if (qualificationQuery.PageNumber > 0)
            {
                var query = qualificationQuery.ToGetChangedQualificationsQuery();

                var response = await Send(query);
                vm = ChangedQualificationsViewModel.Map(
                    response, 
                    statuses, 
                    qualificationQuery);

                if (selectAll)
                {
                    vm.SelectedQualificationIds = vm.ChangedQualifications.Select(q => q.QualificationId).ToList();
                }
            }
            else
            {
                vm = new ChangedQualificationsViewModel();
            }

            vm.Filter = qualificationQuery.ToQualificationFilterViewModel();

            vm.ProcessStatuses = [.. statuses];
            vm.SetBulkActionStatusOptions(statuses.Select(s => (s.Id, s.Name ?? "")));

            if (postedModel != null)
            {
                vm.SelectedQualificationIds = postedModel.SelectedQualificationIds ?? new List<Guid>();
                vm.BulkAction = postedModel.BulkAction;
            }

            return vm;
        }

        [Route("/Review/Changed/ExportData")]
        public async Task<IActionResult> ExportData()
        {
            try
            {
                var result = await Send(new GetChangedQualificationsCsvExportQuery());

                if (result?.QualificationExports != null)
                {
                    return WriteCsvToResponse(result.QualificationExports);
                }
                else
                {
                    return Redirect("/Home/Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the CSV file.");
                return Redirect("/Home/Error");
            }
        }

        private FileContentResult WriteCsvToResponse(IEnumerable<QualificationExport> qualifications)
        {
            var csvData = GenerateCsv(qualifications);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            var fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}-ChangedQualificationsExport.csv";
            return File(bytes, "text/csv", fileName);
        }

        private static string GenerateCsv(IEnumerable<QualificationExport> qualifications)
        {
            using (var writer = new StringWriter())
            using (var csv = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(qualifications);
                return writer.ToString();
            }
        }

        private void ValidatePagingAndNotify(QualificationQuery query)
        {
            if ((query.RecordsPerPage != 10 && query.RecordsPerPage != 20 && query.RecordsPerPage != 50) || query.PageNumber < 0)
            {
                ShowNotificationIfKeyExists(NewQualDataKeys.InvalidPageParams.ToString(),
                    ViewNotificationMessageType.Error,
                    "Invalid parameters.");
            }
        }
    }
}
