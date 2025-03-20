using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
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
        public enum NewQualDataKeys { InvalidPageParams, }

        public ChangedController(ILogger<ChangedController> logger, IMediator mediator) : base(mediator, logger)
        {
            _logger = logger;
            _mediator = mediator;
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
            catch(Exception ex)
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

        public async Task<IActionResult> QualificationDetails([FromQuery] string qualificationReference)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                return Redirect("/Home/Error");
            }

            var result = await Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference });

            var viewModel = MapToViewModel(result);
            return View(viewModel);
        }

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
                LogException(ex);
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
}
