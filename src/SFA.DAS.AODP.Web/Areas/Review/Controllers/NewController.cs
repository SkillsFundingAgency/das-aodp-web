using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Globalization;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]    
    public class NewController : ControllerBase
    {
        private readonly ILogger<NewController> _logger;
        private readonly IMediator _mediator;
        public enum NewQualDataKeys { InvalidPageParams, }

        public NewController(ILogger<NewController> logger, IMediator mediator) : base(mediator, logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index(int pageNumber = 0, int recordsPerPage = 10, string name = "", string organisation = "", string qan = "")
        {
            var viewModel = new NewQualificationsViewModel();
            try
            {

                if (!ModelState.IsValid || (recordsPerPage != 10 && recordsPerPage != 20 && recordsPerPage != 50) || pageNumber < 0)
                {
                    ShowNotificationIfKeyExists(NewQualDataKeys.InvalidPageParams.ToString(), ViewNotificationMessageType.Error, "Invalid parameters.");
                }

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

                    query.Take = recordsPerPage;
                    query.Skip = recordsPerPage * (pageNumber - 1);

                    var response = await Send(query);
                    viewModel = NewQualificationsViewModel.Map(response, organisation, qan, name);
                }                                                                                  
                
                return View(viewModel);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [HttpPost]
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
                        qan = viewModel.Filter.QAN
                    });               
            }
            catch
            {
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
            catch
            {
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
            catch
            {
                return View("Index");
            }
        }


        public async Task<IActionResult> QualificationDetails([FromQuery] string qualificationReference, string status)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                return Redirect("/Home/Error");
            }

            var qualificationsResult = await Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference, Status = status });

            var viewModel = MapToViewModel(qualificationsResult);

            return View(viewModel);
        }


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

        private FileContentResult WriteCsvToResponse(List<QualificationExport> qualifications)
        {            
            var csvData = GenerateCsv(qualifications);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            var fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}-NewQualificationsExport.csv";
            return File(bytes, "text/csv", fileName);            
        }

        private static string GenerateCsv(List<QualificationExport> qualifications)
        {
            using (var writer = new StringWriter())
            using (var csv = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(qualifications);
                return writer.ToString();
            }
        }      

        private static QualificationDetailsViewModel MapToViewModel(GetQualificationDetailsQueryResponse response)
        {
            if (response == null)
            {
                return null;
            }

            return new QualificationDetailsViewModel
            {
                Id = response.Id,
                Status = response.Status,
                Priority = "Not sure",
                Changes = response.VersionFieldChanges,
                QualificationReference = response.Qual.Qan,
                AwardingOrganisation = response.Organisation.NameLegal,
                Title = response.Qual.QualificationName,
                QualificationType = response.Type,
                Level = response.Level,
                ProposedChanges = "none",
                AgeGroup = response.AgeGroup,
                Category = "General Education",
                Subject = response.Specialism,
                SectorSubjectArea = response.Ssa,
                Comments = "No Comment"
                //           ProposedChanges = response.ProposedChanges,
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
