using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.Globalization;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    [Route("review/[controller]")]
    public class QualificationsController : Controller
    {
        private readonly ILogger<QualificationsController> _logger;
        private readonly IMediator _mediator;

        public QualificationsController(ILogger<QualificationsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Index([FromQuery] string status)
        {
            var processedStatus = ProcessAndValidateStatus(status);
            if (processedStatus is IActionResult badRequestResult)
            {
                return badRequestResult;
            }

            IActionResult response = status switch
            {
                "new" => await HandleNewQualifications(),
                _ => BadRequest(new { message = $"Invalid status: {status}" })
            };

            return response;
        }

        [HttpGet("qualificationdetails/{qualificationReference}")]
        [ProducesResponseType(typeof(QualificationDetailsViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        [HttpGet("qualifications/export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQualificationCSVExportData([FromQuery] string? status)
        {
            var processedStatus = ProcessAndValidateStatus(status);
            if (processedStatus is IActionResult badRequestResult)
            {
                return badRequestResult;
            }

            var result = processedStatus switch
            {
                "new" => await HandleNewQualificationCSVExport(),
                // Add more cases for other statuses
                _ => BadRequest(new { message = $"Invalid status: {processedStatus}" })
            };

            if (result is OkObjectResult okResult && okResult.Value is BaseMediatrResponse<GetNewQualificationsCSVExportResponse> response && response.Success)
            {
                return WriteCsvToResponse(response.Value.QualificationExports);
            }

            return result;
        }
        private IActionResult WriteCsvToResponse(List<QualificationExport> qualifications)
        {
            var csvData = GenerateCsv(qualifications);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            var fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}-NewQualificationsExport.csv";
            return File(bytes, "text/csv", fileName);
        }

        private string GenerateCsv(List<QualificationExport> qualifications)
        {
            using (var writer = new StringWriter())
            using (var csv = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(qualifications);
                return writer.ToString();
            }
        }

        private async Task<IActionResult> HandleNewQualifications()
        {
            var result = await _mediator.Send(new GetNewQualificationsQuery());

            if (result == null || !result.Success || result.Value == null)
            {
                _logger.LogWarning("No new qualifications found.");
                return NotFound(new { message = "No new qualifications found" });
            }

            var viewModel = result.Value.NewQualifications.Select(q => new NewQualificationsViewModel
            {
                Id = q.Id,
                Title = q.Title,
                AwardingOrganisation = q.AwardingOrganisation,
                Reference = q.Reference,
                Status = q.Status
            }).ToList();

            return View(viewModel);
        }

        private async Task<IActionResult> HandleNewQualificationCSVExport()
        {
            var result = await _mediator.Send(new GetNewQualificationsCSVExportQuery());

            if (result == null || !result.Success || result.Value == null)
            {
                _logger.LogWarning("No new qualification data found for export.");
                return NotFound(new { message = "No new qualification data found for export" });
            }

            return Ok(result);
        }

        private object ProcessAndValidateStatus(string status)
        {
            status = status?.Trim().ToLower();

            if (string.IsNullOrEmpty(status))
            {
                _logger.LogWarning("Qualification status is missing.");
                return BadRequest(new { message = "Qualification status cannot be empty." });
            }

            return status;
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
    }
}
