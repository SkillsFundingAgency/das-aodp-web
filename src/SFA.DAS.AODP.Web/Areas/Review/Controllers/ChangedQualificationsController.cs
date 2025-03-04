using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Globalization;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    public class ChangedQualificationsController : Controller
    {
        private readonly ILogger<QualificationsController> _logger;
        private readonly IMediator _mediator;

        public ChangedQualificationsController(ILogger<QualificationsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var response = await HandleChangedQualifications();
            return response;
        }

        public async Task<IActionResult> QualificationDetails([FromRoute] string qualificationReference)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                _logger.LogWarning("Qualification reference is empty");
                return BadRequest(new { message = "Qualification reference cannot be empty" });
            }

            var result = await _mediator.Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference });

            if (!result.Success || result.Value == null)
            {
                _logger.LogWarning(result.ErrorMessage);
                return NotFound(); //handle error
            }

            var viewModel = MapToViewModel(result.Value);
            return View(viewModel);
        }

        //[HttpGet("export")]
        public async Task<IActionResult> GetQualificationCSVExportData([FromRoute] string? status)
        {
            var validationResult = ProcessAndValidateStatus(status);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { message = validationResult.ErrorMessage });
            }

            var result = validationResult.ProcessedStatus switch
            {
                "new" => await HandleNewQualificationCSVExport(),
                // Add more cases for other statuses
                _ => new CsvExportResult { Success = false, ErrorMessage = $"Invalid status: {validationResult.ProcessedStatus}" }
            };

            if (result.Success)
            {
                return WriteCsvToResponse(result.QualificationExports);
            }

            return NotFound(new { message = result.ErrorMessage });
        }

        private FileContentResult WriteCsvToResponse(List<QualificationExport> qualifications)
        {
            try
            {
                var csvData = GenerateCsv(qualifications);
                var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
                var fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}-NewQualificationsExport.csv";
                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the CSV file.");
                throw;
            }
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

        private async Task<IActionResult> HandleChangedQualifications()
        {
            //var result = await _mediator.Send(new GetChangedQualificationsQuery());

            //if (result == null || !result.Success || result.Value == null)
            //{
            //    _logger.LogWarning("No new qualifications found.");
            //    return View("NoQualificationsData");
            //}

            //var viewModel = result.Value.ChangedQualifications.Select(q => new ChangedQualificationsViewModel
            //{
            //    Id = q.Id,
            //    Title = q.Title,
            //    AwardingOrganisation = q.AwardingOrganisation,
            //    Reference = q.Reference,
            //    Status = q.Status
            //}).ToList().Take(10);

            var viewModel = new List<ChangedQualificationsViewModel>(){
                new ChangedQualificationsViewModel{AwardingOrganisation="Award A",ChangedFieldNames="Name,Age",Reference="1/4/2",Title="Maths",Status="Changed"},
                 new ChangedQualificationsViewModel{AwardingOrganisation="Award b",ChangedFieldNames="Gender,Tile",Reference="2/42/2",Title="English",Status="Changed"},
                  new ChangedQualificationsViewModel{AwardingOrganisation="Award c",ChangedFieldNames="School,Address,MR",Reference="1/1/2",Title="French",Status="Changed"},
                   new ChangedQualificationsViewModel{AwardingOrganisation="Award d",ChangedFieldNames="Postcode,DOB",Reference="5/42/2",Title="Geography",Status="Changed"},
                    new ChangedQualificationsViewModel{AwardingOrganisation="Award e",ChangedFieldNames="City,Age",Reference="1/4242",Title="Social Economics",Status="Changed"},
                     new ChangedQualificationsViewModel{AwardingOrganisation="Award f",ChangedFieldNames="School,Namee",Reference="1/42/2",Title="Science",Status="Changed"},
                      new ChangedQualificationsViewModel{AwardingOrganisation="Award g",ChangedFieldNames="Title,Age",Reference="1/2/2",Title="Physics",Status="Changed"},
                       new ChangedQualificationsViewModel{AwardingOrganisation="Award h",ChangedFieldNames="Gender,LastKnown",Reference="1/42/2",Title="Something",Status="Changed"},
                        new ChangedQualificationsViewModel{AwardingOrganisation="Award i",ChangedFieldNames="Name,Age",Reference="5/8/2",Title="Anything",Status="Changed"},
                         new ChangedQualificationsViewModel{AwardingOrganisation="Award j",ChangedFieldNames="Title,Name",Reference="1342/2",Title="Literature",Status="Changed"}
            };
            return View(viewModel);
        }

        private async Task<CsvExportResult> HandleNewQualificationCSVExport()
        {
            var result = await _mediator.Send(new GetNewQualificationsCsvExportQuery());

            if (!result.Success || result.Value == null)
            {
                _logger.LogWarning(result.ErrorMessage);
                return new CsvExportResult
                {
                    Success = false,
                    ErrorMessage = result.ErrorMessage
                };
            }

            return new CsvExportResult
            {
                Success = true,
                QualificationExports = result.Value.QualificationExports
            };
        }

        private StatusValidationResult ProcessAndValidateStatus(string status)
        {
            status = status?.Trim().ToLower();

            if (string.IsNullOrEmpty(status))
            {
                _logger.LogWarning("Qualification status is missing.");
                return new StatusValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Qualification status cannot be empty."
                };
            }

            return new StatusValidationResult
            {
                IsValid = true,
                ProcessedStatus = status
            };
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
                Priority = response.Priority,
                Changes = response.Changes,
                QualificationReference = response.QualificationReference,
                AwardingOrganisation = response.AwardingOrganisation,
                Title = response.Title,
                QualificationType = response.QualificationType,
                Level = response.Level,
                ProposedChanges = response.ProposedChanges,
                AgeGroup = response.AgeGroup,
                Category = response.Category,
                Subject = response.Subject,
                SectorSubjectArea = response.SectorSubjectArea,
                Comments = response.Comments
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
