using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Qualification;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Globalization;
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
        private List<string> ReviewerAllowedStatuses { get; set; } = new List<string>()
        {
            "Decision Required",
            "No Action Required",
        };

        public enum NewQualDataKeys { InvalidPageParams, CommentSaved}

        public ChangedController(ILogger<ChangedController> logger, IMediator mediator, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _logger = logger;
            _mediator = mediator;
            this._userHelperService = userHelperService;
        }

        public async Task<IActionResult> Index(List<Guid>? processStatusIds, int pageNumber = 0, int recordsPerPage = 10, string name = "", string organisation = "", string qan = "")
        {
            var viewModel = new ChangedQualificationsViewModel();
            try
            {

                if (!ModelState.IsValid || (recordsPerPage != 10 && recordsPerPage != 20 && recordsPerPage != 50) || pageNumber < 0)
                {
                    ShowNotificationIfKeyExists(NewQualDataKeys.InvalidPageParams.ToString(), ViewNotificationMessageType.Error, "Invalid parameters.");
                }

                var procStatuses = await Send(new GetProcessStatusesQuery());

                // Initial page load will not load records and have a page number of 0
                if (pageNumber > 0)
                {
                    var query = new GetChangedQualificationsQuery();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        query.Name = name;
                    }

                    if (!string.IsNullOrWhiteSpace(organisation))
                    {
                        query.Organisation = organisation;
                    }

                    if (!string.IsNullOrWhiteSpace(qan))
                    {
                        query.QAN = qan;
                    }


                    if (processStatusIds?.Any() ?? false)
                    {
                        query.ProcessStatusIds = processStatusIds;
                    }
                    query.Take = recordsPerPage;
                    query.Skip = recordsPerPage * (pageNumber - 1);

                    var response = await Send(query);
                    viewModel = ChangedQualificationsViewModel.Map(response, procStatuses.ProcessStatuses, organisation, qan, name);
                }
                viewModel.Filter = new NewQualificationFilterViewModel()
                {
                    Organisation = organisation,
                    QualificationName = name,
                    QAN = qan,
                    ProcessStatusIds = processStatusIds
                };
                viewModel.ProcessStatuses = [.. procStatuses.ProcessStatuses];
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
        public async Task<IActionResult> ChangePage(int newPage = 1, int recordsPerPage = 10, string name = "", string organisation = "", string qan = "")
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Index), new
                    {
                        pageNumber = newPage,
                        recordsPerPage = recordsPerPage,
                        name = name,
                        organisation = organisation,
                        qan = qan
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

        [Route("/Review/Changed/QualificationDetails/Timeline")]
        public async Task<IActionResult> QualificationDetailsTimeline([FromQuery] string qualificationReference)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                return Redirect("/Home/Error");
            }

            try
            {
                QualificationDetailsTimelineViewModel discussionHistoryDetailsResult = await Send(new GetDiscussionHistoriesForQualificationQuery { QualificationReference = qualificationReference });
                ChangedQualificationDetailsViewModel qualificationWithVersions = await Send(new GetQualificationDetailWithVersionsQuery { QualificationReference = qualificationReference });

                var latestVersionNumber = qualificationWithVersions.Qual.Versions.Max(i => i.Version) ?? 0;

                var currentVersion = qualificationWithVersions.Qual.Versions.Where(i => i.Version == latestVersionNumber).FirstOrDefault();
                if (latestVersionNumber > 1)
                {
                    for (int? i = latestVersionNumber; i > 1; i--)
                    {
                        if (i != latestVersionNumber)
                            currentVersion = qualificationWithVersions.Qual.Versions.Where(v => v.Version == i).FirstOrDefault();
                        var previousVersion = qualificationWithVersions.Qual.Versions.Where(v => v.Version == i - 1).FirstOrDefault();
                        var keyFieldsChanges = currentVersion.ChangedFieldNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        GetKeyFieldChanges(currentVersion, previousVersion, keyFieldsChanges);

                        if (currentVersion.KeyFieldChanges.Any())
                        {
                            var notes = BuildChangeString(currentVersion);
                            discussionHistoryDetailsResult.QualificationDiscussionHistories.Add(new()
                            {
                                Notes = notes,
                                Title = "Change",
                                UserDisplayName = "OFQUAL Import",
                                Timestamp = currentVersion.InsertedTimestamp
                            });
                        }
                    }
                }
                discussionHistoryDetailsResult.Qan = qualificationReference;
                return View(discussionHistoryDetailsResult);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Redirect("/Home/Error");
                ;
            }
        }

        private static string BuildChangeString(ChangedQualificationDetailsViewModel qualVersion)
        {
            string comment = "";
            foreach (var item in qualVersion.KeyFieldChanges)
            {
                comment += item.Name + "<br/>Was:" + item.Was + "<br/>"
            + "Now:" + item.Now + "<br/><br/>"
                ;
            }
            return comment;
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
                latestVersion.MapFundedOffers(feedbackForQualificationFunding);

                if (latestVersion.Version > 1)
                {
                    var previousVersion = await Send(new GetQualificationVersionQuery() { QualificationReference = qualificationReference, Version = latestVersion.Version - 1 });

                    var keyFieldsChanges = latestVersion?.ChangedFieldNames?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    GetKeyFieldChanges(latestVersion, previousVersion, keyFieldsChanges);
                }
                return View(latestVersion);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return Redirect("/Home/Error");
                ;
            }
        }

        private static void GetKeyFieldChanges(ChangedQualificationDetailsViewModel latestVersion, ChangedQualificationDetailsViewModel previousVersion, string[] keyFieldsChanges)
        {
            foreach (var item in keyFieldsChanges)
            {
                switch (item)
                {
                    case "OrganisationName":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Organisation Name", Was = previousVersion.Organisation.NameOfqual, Now = latestVersion.Organisation.NameOfqual });
                        break;
                    case "Title":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Title", Was = previousVersion.Qual.QualificationName, Now = latestVersion.Qual.QualificationName });
                        break;
                    case "Level":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Level", Was = previousVersion.Level.ToString(), Now = latestVersion.Level.ToString() });
                        break;
                    case "Type":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Type", Was = previousVersion.Type, Now = latestVersion.Type });
                        break;
                    case "TotalCredits":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Total Credits", Was = previousVersion.TotalCredits.ToString(), Now = latestVersion.TotalCredits.ToString() });
                        break;
                    case "Ssa":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "SSA", Was = previousVersion.Ssa.ToString(), Now = latestVersion.Ssa.ToString() });
                        break;
                    case "GradingType":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Grading Type", Was = previousVersion.GradingType.ToString(), Now = latestVersion.GradingType.ToString() });
                        break;
                    case "OfferedInEngland":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Offered In England", Was = previousVersion.OfferedInEngland.ToString(), Now = latestVersion.OfferedInEngland.ToString() });
                        break;
                    case "PreSixteen":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Pre-Sixteen", Was = previousVersion.PreSixteen.ToString(), Now = latestVersion.PreSixteen.ToString() });
                        break;
                    case "SixteenToEighteen":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Sixteen To Eighteen", Was = previousVersion.SixteenToEighteen.ToString(), Now = latestVersion.SixteenToEighteen.ToString() });
                        break;
                    case "EighteenPlus":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Eighteen Plus", Was = previousVersion.EighteenPlus.ToString(), Now = latestVersion.EighteenPlus.ToString() });
                        break;
                    case "NineteenPlus":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Nineteen Plus", Was = previousVersion.NineteenPlus.ToString(), Now = latestVersion.NineteenPlus.ToString() });
                        break;
                    case "FundingInEngland":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Nineteen Plus", Was = previousVersion.FundedInEngland.ToString(), Now = latestVersion.FundedInEngland.ToString() });
                        break;
                    case "GLH":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Guided learning hours (GLH)", Was = previousVersion.Glh.ToString(), Now = latestVersion.Glh.ToString() });
                        break;
                    case "MinimumGlh":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Minimum GLH", Was = previousVersion.MinimumGlh.ToString(), Now = latestVersion.MinimumGlh.ToString() });
                        break;
                    case "TQT":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Total qualification time (TQT)", Was = previousVersion.Tqt.ToString(), Now = latestVersion.Tqt.ToString() });
                        break;
                    case "OperationalEndDate":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Operational End Date", Was = String.Format("{0:MM/dd/yy hh:mm}", previousVersion.OperationalEndDate.ToString()), Now = String.Format("{0:MM/dd/yy HH:mm}", latestVersion.OperationalEndDate.ToString()) });
                        break;
                    case "LastUpdatedDate":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Last updated date", Was = String.Format("{0:MM/dd/yy hh:mm}", previousVersion.LastUpdatedDate.ToString()), Now = String.Format("{0:MM/dd/yy HH:mm}", latestVersion.LastUpdatedDate.ToString()) });

                        break;
                    case "Version":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Version", Was = previousVersion.Version.ToString(), Now = latestVersion.Version.ToString() });

                        break;
                    case "OfferedInternationally":
                        latestVersion.KeyFieldChanges.Add(new() { Name = "Offered Internationally", Was = previousVersion.OfferedInternationally.ToString(), Now = latestVersion.OfferedInternationally.ToString() });
                        break;
                    default:
                        break;
                }
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
                    return RedirectToAction(nameof(QualificationDetails), new { qualificationReference = model.Qual.Qan});
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

    public class FieldMap
    {
        public string DBField { get; set; }
        public string FriendlyName { get; set; }
        public string ClassLocator { get; set; }
    }
}
