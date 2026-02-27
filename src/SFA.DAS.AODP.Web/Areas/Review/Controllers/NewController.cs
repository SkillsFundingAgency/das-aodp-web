using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Application.Commands.Qualification;
using SFA.DAS.AODP.Application.Commands.Qualifications;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Mappers;
using SFA.DAS.AODP.Web.Models.BulkActions;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Globalization;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    [Route("{controller}/{action}")]
    [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
    public class NewController : ControllerBase
    {
        private readonly ILogger<NewController> _logger;
        private readonly IMediator _mediator;
        private readonly IUserHelperService _userHelperService;
        private readonly IOptions<AodpConfiguration> _aodpConfiguration;
        private List<string> ReviewerAllowedStatuses { get; set; } = new List<string>()
        {
            ProcessStatus.DecisionRequired,
            ProcessStatus.NoActionRequired,
        };
        public enum NewQualDataKeys { InvalidPageParams, CommentSaved}

        public NewController(ILogger<NewController> logger, IOptions<AodpConfiguration> configuration, IMediator mediator, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _logger = logger;
            _mediator = mediator;
            _userHelperService = userHelperService;
            _aodpConfiguration = configuration;
        }

        [Route("/Review/New/Index")]
        public async Task<IActionResult> Index(
            [FromQuery]QualificationQuery qualificationQuery,
            bool selectAll = false)
        {
            try
            {
                CheckInvalidParameters(qualificationQuery);

                var viewModel = await BuildIndexViewModelAsync(
                    qualificationQuery,
                    selectAll);

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
        [Route("/Review/New/Search")]
        public async Task<IActionResult> Search(NewQualificationsViewModel viewModel)
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
        [Route("/Review/New/Clear")]
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
        [Route("/Review/New/ChangePage")]
        public async Task<IActionResult> ChangePage([FromQuery]QualificationQuery qualificationQuery, int newPage = 1)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return RedirectToAction(
                        nameof(Index), 
                        qualificationQuery.ToRouteValues(pageNumberOverride: newPage));
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

        [Route("/Review/New/QualificationDetails")]
        public async Task<IActionResult> QualificationDetails([FromQuery] string qualificationReference, [FromQuery] string? returnTo = null)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                return Redirect("/Home/Error");
            }
            try
            {
                ShowNotificationIfKeyExists(NewQualDataKeys.CommentSaved.ToString(), ViewNotificationMessageType.Success, "The comment has been saved.");

                NewQualificationDetailsViewModel model = await Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference });
                model.ProcessStatuses = [.. await GetProcessStatuses()];

                var feedbackForQualificationFunding = await Send(new GetFeedbackForQualificationFundingByIdQuery(model.Id));
                if (feedbackForQualificationFunding != null)
                {
                    model.MapFundedOffers(feedbackForQualificationFunding);
                    model.FundingsOffersOutcomeStatus = feedbackForQualificationFunding.Approved;
                }

                var applications = await Send(new GetApplicationsByQanQuery(model.Qual.Qan));
                if (applications != null)
                    model.Applications = ApplicationMapper.Map(applications);

                model.BackArea = "Review";
                model.BackController = "New";
                model.BackAction = "Index";

                if (!string.IsNullOrWhiteSpace(returnTo) &&
                    string.Equals(returnTo, "QualificationSearch", StringComparison.OrdinalIgnoreCase))
                {
                    model.BackArea = "Review";
                    model.BackController = "QualificationSearch";
                    model.BackAction = "Index";
                    model.ReturnTo = "QualificationSearch";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Redirect("/Home/Error");
            }
        }

        [Route("/Review/New/QualificationDetails")]
        [HttpPost]
        public async Task<IActionResult> QualificationDetails(NewQualificationDetailsViewModel model)
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

        [Route("/Review/New/QualificationDetails/Timeline")]
        public async Task<IActionResult> QualificationDetailsTimeline([FromQuery] string qualificationReference)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(qualificationReference))
                {
                    return Redirect("/Home/Error");
                }

                QualificationDetailsTimelineViewModel result = await Send(new GetDiscussionHistoriesForQualificationQuery { QualificationReference = qualificationReference });
                result.Qan = qualificationReference;
                return View(result);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return RedirectToAction(nameof(QualificationDetails), new { qualificationReference = qualificationReference });
            }
        }

        [Route("/Review/New/ExportData")]
        public async Task<IActionResult> ExportData()
        {
            try
            {
                var result = await Send(new GetNewQualificationsCsvExportQuery());

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

        [HttpPost]
        [Route("/Review/New/ApplyBulkAction")]
        public async Task<IActionResult> ApplyBulkAction(
            NewQualificationsViewModel model,
            QualificationQuery qualificationQuery)
        {

            if (!ModelState.IsValid)
            {
                CheckInvalidParameters(qualificationQuery);

                var viewModel = await BuildIndexViewModelAsync(
                    qualificationQuery,
                    selectAll: false,
                    postedModel: model);

                return View("Index", viewModel);
            }

            await Send(new BulkUpdateQualificationStatusCommand
            {
                QualificationIds = model.SelectedQualificationIds,
                ProcessStatusId = model.BulkAction.ProcessStatusId!.Value,
                Comment = model.BulkAction.Comment,
                UserDisplayName = HttpContext.User?.Identity?.Name
            });

            TempData[BulkActionQualifications.SuccessKey] = true;

            return RedirectToAction(nameof(Index), qualificationQuery.ToRouteValues());
        }

        private void CheckInvalidParameters(QualificationQuery qualificationQuery)
        {
            if ((qualificationQuery.RecordsPerPage != 10 && qualificationQuery.RecordsPerPage != 20 && qualificationQuery.RecordsPerPage != 50) || qualificationQuery.PageNumber < 0)
            {
                ShowNotificationIfKeyExists(NewQualDataKeys.InvalidPageParams.ToString(),
                    ViewNotificationMessageType.Error,
                    "Invalid parameters.");
            }
        }

        private bool CheckUserIsAbleToSetStatus(NewQualificationDetailsViewModel model, Guid procStatusId)
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

        private FileContentResult WriteCsvToResponse(IEnumerable<QualificationExport> qualifications)
        {
            var csvData = GenerateCsv(qualifications);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            var fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}-NewQualificationsExport.csv";
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

        private async Task<NewQualificationsViewModel> BuildIndexViewModelAsync(
            QualificationQuery qualificationQuery,
            bool selectAll = false,
            NewQualificationsViewModel? postedModel = null)
        {
            var procStatuses = await Send(new GetProcessStatusesQuery());
            var statuses = procStatuses.ProcessStatuses ?? new List<GetProcessStatusesQueryResponse.ProcessStatus>();

            NewQualificationsViewModel vm;

            if (qualificationQuery.PageNumber > 0)
            {
                var query = qualificationQuery.ToGetNewQualificationsQuery();   
                var response = await Send(query);
                vm = NewQualificationsViewModel.Map(
                    response, 
                    statuses, 
                    qualificationQuery);

                if (selectAll)
                {
                    vm.SelectedQualificationIds = vm.NewQualifications.Select(q => q.QualificationId).ToList();
                }
            }
            else
            {
                vm = new NewQualificationsViewModel();
            }

            vm.FindRegulatedQualificationUrl = _aodpConfiguration.Value.FindRegulatedQualificationUrl;

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

        private class CsvExportResult
        {
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
            public List<QualificationExport> QualificationExports { get; set; } = new List<QualificationExport>();
        }

        private class StatusValidationResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public string? ProcessedStatus { get; set; }
        }
    }
}
