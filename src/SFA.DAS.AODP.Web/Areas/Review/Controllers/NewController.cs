using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Qualification;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Models;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        private List<string> ReviewerAllowedStatuses { get; set; } = new List<string>()
        {
            "Decision Required",
            "No Action Required",
        };
        public enum NewQualDataKeys { InvalidPageParams, }

        public NewController(ILogger<NewController> logger, IMediator mediator, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _logger = logger;
            _mediator = mediator;
            _userHelperService = userHelperService;
        }

        [Route("/Review/New/Index")]
        public async Task<IActionResult> Index(List<Guid>? processStatusIds, int pageNumber = 0, int recordsPerPage = 10, string name = "", string organisation = "", string qan = "")
        {
            var viewModel = new NewQualificationsViewModel();
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
                    var query = new GetNewQualificationsQuery();
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
                        query.ProcessStatusFilter = new Domain.Models.ProcessStatusFilter() { ProcessStatusIds = processStatusIds };
                    }

                    query.Take = recordsPerPage;
                    query.Skip = recordsPerPage * (pageNumber - 1);

                    var response = await Send(query);
                    viewModel = NewQualificationsViewModel.Map(response, procStatuses.ProcessStatuses, organisation, qan, name);
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
            catch(Exception ex)
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

        [Route("/Review/New/QualificationDetails")]
        public async Task<IActionResult> QualificationDetails([FromQuery] string qualificationReference)
        {            
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                return Redirect("/Home/Error");
            }
            try
            {
                NewQualificationDetailsViewModel result = await Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference });
                result.ProcessStatuses = [.. await GetProcessStatuses()];
                return View(result);
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

                NewQualificationDetailsTimelineViewModel result = await Send(new GetDiscussionHistoriesForQualificationQuery { QualificationReference = qualificationReference });
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
