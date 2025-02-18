using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Filters;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;

namespace SFA.DAS.AODP.Web.Controllers.Application
{
    [ValidateOrganisation]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationAnswersValidator _validator;
        private readonly IFileService _fileService;
        public ApplicationsController(IMediator mediator, IApplicationAnswersValidator validator, ILogger<FormsController> logger, IFileService fileService) : base(mediator, logger)
        {
            _validator = validator;
            _fileService = fileService;
        }

        [HttpGet]
        [Route("organisations/{organisationId}")]
        public async Task<IActionResult> Index(Guid organisationId)
        {
            try
            {
                var response = await Send(new GetApplicationsByOrganisationIdQuery(organisationId));
                ListApplicationsViewModel model = ListApplicationsViewModel.Map(response, organisationId);
                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [HttpGet]
        [Route("organisations/{organisationId}/forms")]
        public async Task<IActionResult> AvailableFormsAsync(Guid organisationId)
        {
            try
            {
                var formsResponse = await Send(new GetApplicationFormsQuery());
                ListAvailableFormsViewModel model = ListAvailableFormsViewModel.Map(formsResponse, organisationId);
                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [HttpGet]
        [Route("organisations/{organisationId}/forms/{formVersionId}/Create")]
        public async Task<IActionResult> Create(Guid organisationId, Guid formVersionId)
        {
            try
            {
                var formVersion = await Send(new GetFormVersionByIdQuery(formVersionId));

                return View(new CreateApplicationViewModel()
                {
                    OrganisationId = organisationId,
                    FormVersionId = formVersionId,
                    FormTitle = formVersion.Title
                });
            }
            catch
            {
                return Redirect("/Home/Error");
            }
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
                OrganisationId = createApplicationViewModel.OrganisationId,
                QualificationNumber = createApplicationViewModel.QualificationNumber,
            };

            try
            {
                var response = await Send(request);

                return RedirectToAction(nameof(ViewApplication), new { organisationId = createApplicationViewModel.OrganisationId, applicationId = response.Id, formVersionId = createApplicationViewModel.FormVersionId });
            }
            catch
            {
                return View(createApplicationViewModel);
            }
        }


        [ValidateApplication]
        [HttpGet]
        [Route("organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}")]
        public async Task<IActionResult> ViewApplication(Guid organisationId, Guid applicationId, Guid formVersionId)
        {
            try
            {
                var formsResponse = await Send(new GetApplicationFormByIdQuery(formVersionId));

                var statusResponse = await Send(new GetApplicationFormStatusByApplicationIdQuery(formVersionId, applicationId));

                ApplicationFormViewModel model = ApplicationFormViewModel.Map(formsResponse, statusResponse, formVersionId, organisationId, applicationId);

                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [ValidateApplication]
        [HttpGet]
        [Route("organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/sections/{sectionId}")]
        public async Task<IActionResult> ViewApplicationSection(Guid organisationId, Guid applicationId, Guid sectionId, Guid formVersionId)
        {
            try
            {
                var sectionResponse = await Send(new GetApplicationSectionByIdQuery(sectionId, formVersionId));

                var sectionStatus = await Send(new GetApplicationSectionStatusByApplicationIdQuery(sectionId, formVersionId, applicationId));

                ApplicationSectionViewModel model = ApplicationSectionViewModel.Map(sectionResponse, sectionStatus, organisationId, formVersionId, sectionId, applicationId);

                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [ValidateApplication]
        [HttpGet]
        [Route("organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/sections/{sectionId}/pages/{pageOrder}")]
        public async Task<IActionResult> ApplicationPage(Guid organisationId, Guid applicationId, Guid sectionId, int pageOrder, Guid formVersionId)
        {
            try
            {
                Func<string, List<UploadedBlob>> fetchBlobFunc = path => _fileService.ListBlobs(path);

                var request = new GetApplicationPageByIdQuery()
                {
                    FormVersionId = formVersionId,
                    PageOrder = pageOrder,
                    SectionId = sectionId,
                };

                var response = await Send(request);

                var answers = await Send(new GetApplicationPageAnswersByPageIdQuery(applicationId, response.Id, sectionId, formVersionId));

                ApplicationPageViewModel viewModel = ApplicationPageViewModel.MapToViewModel(response, applicationId, formVersionId, sectionId, organisationId, answers, fetchBlobFunc);

                return View(viewModel);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [ValidateApplication]
        [HttpPost]
        [Route("organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/sections/{sectionId}/pages/{pageOrder}")]
        public async Task<IActionResult> ApplicationPageAsync([FromForm] ApplicationPageViewModel model)
        {
            Func<string, List<UploadedBlob>> fetchBlobFunc = path => _fileService.ListBlobs(path);

            var request = new GetApplicationPageByIdQuery()
            {
                FormVersionId = model.FormVersionId,
                PageOrder = model.Order,
                SectionId = model.SectionId,
            };

            var response = await Send(request);

            try
            {
                if (!string.IsNullOrEmpty(model.RemoveFile))
                {
                    if (!model.RemoveFile.StartsWith(model.ApplicationId.ToString()))
                    {
                        return BadRequest();
                    }
                    await _fileService.DeleteFileAsync(model.RemoveFile);
                    model = ApplicationPageViewModel.RepopulatePageDataOnViewModel(response, model, fetchBlobFunc);
                    return View(model);

                }
                _validator.ValidateApplicationPageAnswers(ModelState, response, model);

                if (!ModelState.IsValid)
                {
                    model = ApplicationPageViewModel.RepopulatePageDataOnViewModel(response, model, fetchBlobFunc);
                    return View(model);
                }

                await HandleFileUploads(model);

                var command = ApplicationPageViewModel.MapToCommand(model, response);

                var commandResponse = await Send(command);

                bool endSection = command.Routing?.EndSection == true || response.TotalSectionPages == response.Order;
                if (endSection) return RedirectToAction(nameof(ViewApplicationSection), new { organisationId = model.OrganisationId, applicationId = model.ApplicationId, sectionId = model.SectionId, formVersionId = model.FormVersionId });


                return RedirectToAction(nameof(ApplicationPage),
                    new
                    {
                        organisationId = model.OrganisationId,
                        applicationId = model.ApplicationId,
                        sectionId = model.SectionId,
                        pageOrder = command.Routing?.NextPageOrder ?? response.Order + 1,
                        formVersionId = model.FormVersionId
                    });
            }
            catch
            {
                model = ApplicationPageViewModel.RepopulatePageDataOnViewModel(response, model, fetchBlobFunc);

                return View(model);
            }
        }


        private async Task HandleFileUploads(ApplicationPageViewModel viewModel)
        {
            foreach (var question in viewModel.Questions?.Where(q => q.Type == AODP.Models.Forms.QuestionType.File) ?? [])
            {
                foreach (var file in question.Answer.FormFiles ?? [])
                {
                    using var stream = file.OpenReadStream();
                    await _fileService.UploadFileAsync($"{viewModel.ApplicationId}/{question.Id}", file.FileName, stream, file.ContentType);
                }
            }
        }
    }
}


