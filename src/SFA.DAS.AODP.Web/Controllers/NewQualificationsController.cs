using Microsoft.AspNetCore.Mvc;
using MediatR;
using SFA.DAS.AODP.Application.Queries.Test;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;

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
            var result = await _mediator.Send(new GetNewQualificationsQuery());

            if (!result.Success)
            {
                return NotFound("Error"); // Handle errors properly
            }

            var viewModel = result.NewQualifications.Select(q => new NewQualificationsViewModel
            {
                Id = q.Id,
                Title = q.Title,
                AwardingOrganisation = q.AwardingOrganisation,
                Reference = q.Reference,
                Status = q.Status

            }).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> QualificationDetails([FromQuery]string qualificationReference)
        {
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {
                return BadRequest(new { message = "Qualification reference cannot be empty" });
            }

            var result = await _mediator.Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference });

            if (!result.Success)
            {
                return NotFound(); 
            }

            var viewModel = new QualificationDetailsViewModel
            {
                Id = result.Id,
                Status = result.Status,
                Priority = result.Priority,
                Changes = result.Changes,
                QualificationReference = result.QualificationReference,
                AwardingOrganisation = result.AwardingOrganisation,
                Title = result.Title,
                QualificationType = result.QualificationType,
                Level = result.Level,
                ProposedChanges = result.ProposedChanges,
                AgeGroup = result.AgeGroup,
                Category = result.Category,
                Subject = result.Subject,
                SectorSubjectArea = result.SectorSubjectArea,
                Comments = result.Comments
            };

            return View(viewModel);
        }

    }
}
