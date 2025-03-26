using Azure;
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
using System.Reflection;
using static SFA.DAS.AODP.Application.Queries.Qualifications.GetQualificationDetailsQueryResponse;
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

        public enum NewQualDataKeys { InvalidPageParams, }

        public ChangedController(ILogger<ChangedController> logger, IMediator mediator, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _logger = logger;
            _mediator = mediator;
            this._userHelperService = userHelperService;
        }

        public async Task<IActionResult> Index(int pageNumber = 0, int recordsPerPage = 10, string name = "", string organisation = "", string qan = "")
        {
            var viewModel = new ChangedQualificationsViewModel();
            try
            {

                if (!ModelState.IsValid || (recordsPerPage != 10 && recordsPerPage != 20 && recordsPerPage != 50) || pageNumber < 0)
                {
                    ShowNotificationIfKeyExists(NewQualDataKeys.InvalidPageParams.ToString(), ViewNotificationMessageType.Error, "Invalid parameters.");
                }

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

                    query.Take = recordsPerPage;
                    query.Skip = recordsPerPage * (pageNumber - 1);

                    var response = await Send(query);
                    viewModel = ChangedQualificationsViewModel.Map(response, organisation, qan, name);
                }

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
                    qan = viewModel.Filter.QAN
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

            NewQualificationDetailsTimelineViewModel result = await Send(new GetDiscussionHistoriesForQualificationQuery { QualificationReference = qualificationReference });
            result.Qan = qualificationReference;
            return View(result);
        }


        [Route("/Review/Changed/QualificationDetails")]
        public async Task<IActionResult> QualificationDetails([FromQuery] string qualificationReference)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                return Redirect("/Home/Error");
            }

            ChangedQualificationDetailsViewModel latestVersion = await Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference });
            latestVersion.ProcessStatuses = [.. await GetProcessStatuses()];

            if (latestVersion.Version > 1)
            {
                var previousVersion = await Send(new GetQualificationVersionQuery() { QualificationReference = qualificationReference, Version = latestVersion.Version - 1 });

                var keyFieldsChanges = latestVersion.ChangedFieldNames.Split(',', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
                foreach (var item in keyFieldsChanges)
                {
                    switch (item)
                    {
                        case "organisationName":
                            latestVersion.KeyFieldChanges.Add(new() { Name = "Organisation Name", Was = previousVersion.Organisation.NameLegal, Now = latestVersion.Organisation.NameLegal });
                        break;
                        case "Title":
                            latestVersion.KeyFieldChanges.Add(new() { Name = "Title", Was = previousVersion.Qual.QualificationName, Now = latestVersion.Qual.QualificationName});
                            break;
                        case "Type":
                            latestVersion.KeyFieldChanges.Add(new() { Name = "Type", Was = previousVersion.Type, Now = latestVersion.Type });
                            break;
                        case "TotalCredits":
                            latestVersion.KeyFieldChanges.Add(new() { Name = "Total Credits", Was = previousVersion.TotalCredits.ToString(), Now = latestVersion.TotalCredits.ToString() });
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
                        case "MinimumGLH":
                            latestVersion.KeyFieldChanges.Add(new() { Name = "Minimum GLH", Was = previousVersion.MinimumGlh.ToString(), Now = latestVersion.MinimumGlh.ToString() });
                            break;
                        case "TQT":
                            latestVersion.KeyFieldChanges.Add(new() { Name = "TQT", Was = previousVersion.Tqt.ToString(), Now = latestVersion.Tqt.ToString() });
                            break;
                        case "OperationalEndDate":
                            latestVersion.KeyFieldChanges.Add(new() { Name = "Operational End Date", Was = previousVersion.OperationalEndDate.ToString(), Now = latestVersion.OperationalEndDate.ToString() });
                            break;
                        case "LastUpdatedDate":
                            latestVersion.KeyFieldChanges.Add(new() { Name = "Last updated date", Was = previousVersion.LastUpdatedDate.ToString(), Now = latestVersion.LastUpdatedDate.ToString() });

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


            return View(latestVersion);
        }



        [Route("/Review/Changed/QualificationDetails")]
        [HttpPost]
        public async Task<IActionResult> QualificationDetails(ChangedQualificationDetailsViewModel model)
        {
            Guid? procStatus = model.AdditionalActions.ProcessStatusId;
            if (string.IsNullOrEmpty(model.AdditionalActions.Note) && procStatus.HasValue)
            {
                model.ProcessStatuses = [.. await GetProcessStatuses()];
                return View(model);
            }
            if (!procStatus.HasValue)
            {
                await Send(new AddQualificationDiscussionHistoryCommand
                {
                    QualificationReference = model.Qual.Qan,
                    Notes = model.AdditionalActions.Note,
                    UserDisplayName = HttpContext.User?.Identity?.Name
                });
                return RedirectToAction(nameof(QualificationDetails), new { qualificationReference = model.Qual.Qan });
            }

            model.ProcessStatuses = [.. await GetProcessStatuses()];
            if (!CheckUserIsAbleToSetStatus(model, procStatus.Value))
                return View(model);

            await Send(new UpdateQualificationStatusCommand
            {
                QualificationReference = model.Qual.Qan,
                ProcessStatusId = procStatus.Value,
                Version=model.Version,
                Notes = model.AdditionalActions.Note,
                UserDisplayName = HttpContext.User?.Identity?.Name
            });
            return RedirectToAction(nameof(QualificationDetails), new { qualificationReference = model.Qual.Qan });
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

        private static ChangedQualificationDetailsViewModel MapToViewModel(GetQualificationDetailsQueryResponse response)
        {
            if (response == null)
            {
                return null;
            }

            return new ChangedQualificationDetailsViewModel
            {
                //Id = response.Id,
                //Status = response.Status,
                //Priority = response.Priority,
                //Changes = response.Changes,
                //QualificationReference = response.QualificationReference,
                //AwardingOrganisation = response.AwardingOrganisation,
                //Title = response.Title,
                //QualificationType = response.QualificationType,
                //Level = response.Level,
                //ProposedChanges = response.ProposedChanges,
                //AgeGroup = response.AgeGroup,
                //Category = response.Category,
                //Subject = response.Subject,
                //SectorSubjectArea = response.SectorSubjectArea,
                //Comments = response.Comments
            };
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
