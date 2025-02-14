using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Application.Queries.Test;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    public class NewQualificationsController : Controller
    {
        private readonly ILogger<NewQualificationsController> _logger;
        private readonly IMediator _mediator;

        public NewQualificationsController(ILogger<NewQualificationsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Getting all new qualifications");

            var result = await _mediator.Send(new GetNewQualificationsQuery());

            if (!result.Success || result.Value == null)
            {
                _logger.LogWarning("No new qualifications found");
                return NotFound("Error"); // Handle errors properly
            }

            _logger.LogInformation("Successfully retrieved new qualifications");

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

        public async Task<IActionResult> QualificationDetails([FromQuery] string qualificationReference)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                _logger.LogWarning("Qualification reference is empty");
                return BadRequest(new { message = "Qualification reference cannot be empty" });
            }

            _logger.LogInformation("Getting details for qualification reference: {QualificationReference}", qualificationReference);

            var result = await _mediator.Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference });

            if (!result.Success || result.Value == null)
            {
                _logger.LogWarning(result.ErrorMessage);
                return NotFound();
            }

            _logger.LogInformation("Successfully retrieved details for qualification reference: {QualificationReference}", qualificationReference);

            var viewModel = MapToViewModel(result.Value);

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
