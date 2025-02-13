using Microsoft.AspNetCore.Mvc;
using MediatR;
using SFA.DAS.AODP.Application.Queries.Test;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;
using Azure;

namespace SFA.DAS.AODP.Web.Controllers
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

            var viewModel = result.Value.Value.NewQualifications.Select(q => new NewQualificationsViewModel
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
                _logger.LogWarning("No details found for qualification reference: {QualificationReference}", qualificationReference);
                return NotFound();
            }

            _logger.LogInformation("Successfully retrieved details for qualification reference: {QualificationReference}", qualificationReference);

            var viewModel = MapToViewModel(result.Value);

            return View(viewModel);
        }

        private static QualificationDetailsViewModel MapToViewModel(GetQualificationDetailsQueryResponse response)
        {
            if (response == null || response.Value == null)
            {
                return null;
            }

            return new QualificationDetailsViewModel
            {
                Id = response.Value.Id,
                Status = response.Value.Status,
                Priority = response.Value.Priority,
                Changes = response.Value.Changes,
                QualificationReference = response.Value.QualificationReference,
                AwardingOrganisation = response.Value.AwardingOrganisation,
                Title = response.Value.Title,
                QualificationType = response.Value.QualificationType,
                Level = response.Value.Level,
                ProposedChanges = response.Value.ProposedChanges,
                AgeGroup = response.Value.AgeGroup,
                Category = response.Value.Category,
                Subject = response.Value.Subject,
                SectorSubjectArea = response.Value.SectorSubjectArea,
                Comments = response.Value.Comments
            };
        }
    }
}
