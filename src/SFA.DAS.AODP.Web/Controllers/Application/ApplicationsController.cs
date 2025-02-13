using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.Controllers.Application
{
    public class ApplicationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IApplicationAnswersValidator _validator;

        public ApplicationsController(IMediator mediator, IApplicationAnswersValidator validator)
        {
            _mediator = mediator;
            _validator = validator;
        }

        [HttpGet]
        [Route("organisations/{organisationId}")]
        public async Task<IActionResult> Index(Guid organisationId)
        {
            var applications = await _mediator.Send(new GetApplicationsByOrganisationIdQuery(organisationId));
            ListApplicationsViewModel model = ListApplicationsViewModel.Map(applications.Value, organisationId);
            return View(model);
        }

        [HttpGet]
        [Route("organisations/{organisationId}/forms")]
        public async Task<IActionResult> AvailableFormsAsync(Guid organisationId)
        {
            var formsResponse = await _mediator.Send(new GetApplicationFormsQuery());
            ListAvailableFormsViewModel model = ListAvailableFormsViewModel.Map(formsResponse.Value, organisationId);
            return View(model);
        }

        [HttpGet]
        [Route("organisations/{organisationId}/forms/{formVersionId}/Create")]
        public async Task<IActionResult> Create(Guid organisationId, Guid formVersionId)
        {
            var formVersion = await _mediator.Send(new GetFormVersionByIdQuery(formVersionId));
            if (!formVersion.Success) return StatusCode(StatusCodes.Status500InternalServerError);

            return View(new CreateApplicationViewModel()
            {
                OrganisationId = organisationId,
                FormVersionId = formVersionId,
                FormTitle = formVersion.Value.Title
            });
        }


        [HttpPost]
        [Route("organisations/{organisationId}/forms/{formVersionId}/Create")]
        public async Task<IActionResult> Create(CreateApplicationViewModel createApplicationViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(createApplicationViewModel);
            }

            var request = new CreateApplicationCommand()
            {
                Title = createApplicationViewModel.Name,
                FormVersionId = createApplicationViewModel.FormVersionId,
                Owner = createApplicationViewModel.Owner,
            };

            var response = await _mediator.Send(request);

            if (!response.Success) return NotFound();

            return RedirectToAction(nameof(ViewApplication), new { organisationId = createApplicationViewModel.OrganisationId, applicationId = response.Value.Id, formVersionId = createApplicationViewModel.FormVersionId });
        }


        [HttpGet]
        [Route("organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}")]
        public async Task<IActionResult> ViewApplication(Guid organisationId, Guid applicationId, Guid formVersionId)
        {
            var formsResponse = await _mediator.Send(new GetApplicationFormByIdQuery(formVersionId));

            var statusResponse = await _mediator.Send(new GetApplicationFormStatusByApplicationIdQuery(formVersionId, applicationId));

            ApplicationFormViewModel model = ApplicationFormViewModel.Map(formsResponse.Value, statusResponse.Value, formVersionId, organisationId, applicationId);

            return View(model);

        }


        [HttpGet]
        [Route("organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/sections/{sectionId}")]
        public async Task<IActionResult> ViewApplicationSection(Guid organisationId, Guid applicationId, Guid sectionId, Guid formVersionId)
        {
            var sectionResponse = await _mediator.Send(new GetApplicationSectionByIdQuery(sectionId, formVersionId));

            var sectionStatus = await _mediator.Send(new GetApplicationSectionStatusByApplicationIdQuery(sectionId, formVersionId, applicationId));

            ApplicationSectionViewModel model = ApplicationSectionViewModel.Map(sectionResponse.Value, sectionStatus.Value, organisationId, formVersionId, sectionId, applicationId);

            return View(model);
        }

        [HttpGet]
        [Route("organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/sections/{sectionId}/pages/{pageOrder}")]
        public async Task<IActionResult> ApplicationPage(Guid organisationId, Guid applicationId, Guid sectionId, int pageOrder, Guid formVersionId)
        {
            var request = new GetApplicationPageByIdQuery()
            {
                FormVersionId = formVersionId,
                PageOrder = pageOrder,
                SectionId = sectionId,
            };

            var response = await _mediator.Send(request);
            if (!response.Success) return NotFound();

            var answers = await _mediator.Send(new GetApplicationPageAnswersByPageIdQuery(applicationId, response.Value.Id, sectionId, formVersionId));
            if (!answers.Success) return NotFound();

            ApplicationPageViewModel viewModel = ApplicationPageViewModel.MapToViewModel(response.Value, applicationId, formVersionId, sectionId, organisationId, answers.Value);

            return View(viewModel);
        }

        [HttpPost]
        [Route("organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/sections/{sectionId}/pages/{pageOrder}")]
        public async Task<IActionResult> ApplicationPageAsync(ApplicationPageViewModel model)
        {
            var request = new GetApplicationPageByIdQuery()
            {
                FormVersionId = model.FormVersionId,
                PageOrder = model.Order,
                SectionId = model.SectionId,
            };

            var response = await _mediator.Send(request);
            if (!response.Success) return NotFound();

            _validator.ValidateApplicationPageAnswers(ModelState, response.Value, model);

            if (!ModelState.IsValid)
            {
                model = ApplicationPageViewModel.RepopulatePageDataOnViewModel(response.Value, model);
                return View(model);
            }

            var command = ApplicationPageViewModel.MapToCommand(model, response.Value);

            var commandResponse = await _mediator.Send(command);

            if (!commandResponse.Success) return NotFound();

            bool endSection = command.Routing?.EndSection == true || response.Value.TotalSectionPages == response.Value.Order;
            if (endSection) return RedirectToAction(nameof(ViewApplicationSection), new { organisationId = model.OrganisationId, applicationId = model.ApplicationId, sectionId = model.SectionId, formVersionId = model.FormVersionId });


            return RedirectToAction(nameof(ApplicationPage),
                new
                {
                    organisationId = model.OrganisationId,
                    applicationId = model.ApplicationId,
                    sectionId = model.SectionId,
                    pageOrder = command.Routing?.NextPageOrder ?? response.Value.Order + 1,
                    formVersionId = model.FormVersionId
                });
        }

    }
}


