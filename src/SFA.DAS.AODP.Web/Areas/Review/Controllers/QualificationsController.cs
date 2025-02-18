using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;

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
        public async Task<IActionResult> Index([FromQuery] string status)
        {
            status = status?.Trim().ToLower();

            if (string.IsNullOrEmpty(status))
            {
                _logger.LogWarning("Qualification status is missing.");
                return BadRequest(new { message = "Qualification status cannot be empty." });
            }

            IActionResult response = status switch
            {
                "new" => await HandleNewQualifications(),
                _ => BadRequest(new { message = $"Invalid status: {status}" })
            };

            return response;
        }

        [HttpGet("qualificationdetails/{qualificationReference}")]
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
