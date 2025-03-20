using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Areas.Admin.Controllers.FormBuilder;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Filters;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Application;
using SFA.DAS.AODP.Web.Validators;
using System.Reflection;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Apply.Controllers
{
    [Area("Apply")]
    [Authorize(Policy = PolicyConstants.IsApplyUser)]
    [ValidateOrganisation]
    public class ApplicationsController : ControllerBase
    {
        public enum UpdateKeys
        {
            ApplicationDeletedKey
        }

        private readonly IApplicationAnswersValidator _validator;
        private readonly IFileService _fileService;
        private readonly IUserHelperService _userHelperService;

        public ApplicationsController(IMediator mediator, IApplicationAnswersValidator validator, ILogger<ApplicationsController> logger, IFileService fileService, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _validator = validator;
            _fileService = fileService;
            _userHelperService = userHelperService;
        }

        [HttpGet]
        [Route("apply/applications")]
        public async Task<IActionResult> Index()
        {
            var organisationId = Guid.Parse(_userHelperService.GetUserOrganisationId());
            var response = await Send(new GetApplicationsByOrganisationIdQuery(organisationId));
            ListApplicationsViewModel model = ListApplicationsViewModel.Map(response, organisationId);

            ShowNotificationIfKeyExists(UpdateKeys.ApplicationDeletedKey.ToString(), ViewNotificationMessageType.Success, "The application has been deleted.");

            return View(model);
        }

        [HttpGet]
        [Route("apply/organisations/{organisationId}/forms")]
        public async Task<IActionResult> AvailableFormsAsync(Guid organisationId)
        {
            var formsResponse = await Send(new GetApplicationFormsQuery());
            ListAvailableFormsViewModel model = ListAvailableFormsViewModel.Map(formsResponse, organisationId);
            return View(model);
        }

        [HttpGet]
        [Route("apply/organisations/{organisationId}/forms/{formVersionId}/Create")]
        public async Task<IActionResult> Create(Guid organisationId, Guid formVersionId)
        {
            var formVersion = await Send(new GetFormVersionByIdQuery(formVersionId));

            return View(new CreateApplicationViewModel()
            {
                OrganisationId = organisationId,
                FormVersionId = formVersionId,
                FormTitle = formVersion.Title
            });
        }


        [HttpPost]
        [Route("apply/organisations/{organisationId}/forms/{formVersionId}/Create")]
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
                OrganisationId = Guid.Parse(_userHelperService.GetUserOrganisationId()),
                QualificationNumber = createApplicationViewModel.QualificationNumber,
                OrganisationName = _userHelperService.GetUserOrganisationName(),
                OrganisationUkprn = _userHelperService.GetUserOrganisationUkPrn()
            };

            try
            {
                var response = await Send(request);

                return RedirectToAction(nameof(ViewApplication), new { organisationId = createApplicationViewModel.OrganisationId, applicationId = response.Id, formVersionId = createApplicationViewModel.FormVersionId });
            }
            catch (Exception ex)
            {
                LogException(ex);
                return View(createApplicationViewModel);
            }
        }

        [ValidateApplication]
        [HttpGet]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/Edit")]
        public async Task<IActionResult> Edit(Guid organisationId, Guid applicationId)
        {
            var application = await Send(new GetApplicationByIdQuery(applicationId));

            return View(new EditApplicationViewModel()
            {
                OrganisationId = organisationId,
                FormVersionId = application.FormVersionId,
                Name = application.Name,
                ApplicationId = applicationId,
                Owner = application.Owner,
                QualificationNumber = application.QualificationNumber
            });
        }


        [ValidateApplication]
        [HttpPost]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/Edit")]
        public async Task<IActionResult> Edit(EditApplicationViewModel editApplicationViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editApplicationViewModel);
            }

            var request = new EditApplicationCommand()
            {
                Title = editApplicationViewModel.Name,
                ApplicationId = editApplicationViewModel.ApplicationId,
                Owner = editApplicationViewModel.Owner,
                QualificationNumber = editApplicationViewModel.QualificationNumber,

            };

            try
            {
                var response = await Send(request);

                return RedirectToAction(nameof(ViewApplication), new
                {
                    organisationId = editApplicationViewModel.OrganisationId,
                    applicationId = editApplicationViewModel.ApplicationId,
                    formVersionId = editApplicationViewModel.FormVersionId
                });
            }
            catch (Exception ex)
            {
                LogException(ex);
                return View(editApplicationViewModel);
            }
        }


        [ValidateApplication]
        [HttpGet]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}")]
        public async Task<IActionResult> ViewApplication(Guid organisationId, Guid applicationId, Guid formVersionId)
        {
            var formsResponse = await Send(new GetApplicationFormByIdQuery(formVersionId));

            var statusResponse = await Send(new GetApplicationFormStatusByApplicationIdQuery(formVersionId, applicationId));

            ApplicationFormViewModel model = ApplicationFormViewModel.Map(formsResponse, statusResponse, formVersionId, organisationId, applicationId);

            return View(model);
        }

        [ValidateApplication]
        [HttpGet]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/sections/{sectionId}")]
        public async Task<IActionResult> ViewApplicationSection(Guid organisationId, Guid applicationId, Guid sectionId, Guid formVersionId)
        {
            var sectionResponse = await Send(new GetApplicationSectionByIdQuery(sectionId, formVersionId));

            var sectionStatus = await Send(new GetApplicationSectionStatusByApplicationIdQuery(sectionId, formVersionId, applicationId));

            ApplicationSectionViewModel model = ApplicationSectionViewModel.Map(sectionResponse, sectionStatus, organisationId, formVersionId, sectionId, applicationId);

            return View(model);
        }

        [ValidateApplication]
        [HttpGet]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/sections/{sectionId}/pages/{pageOrder}")]
        public async Task<IActionResult> ApplicationPage(Guid organisationId, Guid applicationId, Guid sectionId, int pageOrder, Guid formVersionId)
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

        [ValidateApplication]
        [HttpPost]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/sections/{sectionId}/pages/{pageOrder}")]
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
                var command = ApplicationPageViewModel.MapToCommand(model, response);

                var commandResponse = await Send(command);

                await HandleFileUploads(model);

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
            catch (Exception ex)
            {
                LogException(ex);

                model = ApplicationPageViewModel.RepopulatePageDataOnViewModel(response, model, fetchBlobFunc);
                return View(model);
            }
        }


        #region Delete
        [ValidateApplication]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/delete")]
        public async Task<IActionResult> Delete(Guid applicationId)
        {
            var query = new GetApplicationByIdQuery(applicationId);
            var response = await Send(query);
            return View(new DeleteApplicationViewModel()
            {
                ApplicationId = applicationId,
                ApplicationReference = response.Reference,
                OrganisationId = response.OrganisationId,
                ApplicationName = response.Name,
                FormVersionId = response.FormVersionId
            });
        }

        [ValidateApplication]
        [HttpPost]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/delete")]
        public async Task<IActionResult> Delete(DeleteApplicationViewModel model)
        {
            try
            {
                var command = new DeleteApplicationCommand(model.ApplicationId);

                await DeleteApplicationFiles(model.ApplicationId);
                await Send(command);

                TempData[UpdateKeys.ApplicationDeletedKey.ToString()] = true;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                LogException(ex);
                return View(model);
            }
        }


        #endregion

        #region Submit
        [ValidateApplication]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/submit")]
        public async Task<IActionResult> Submit(Guid applicationId)
        {
            var query = new GetApplicationByIdQuery(applicationId);
            var response = await Send(query);
            return View(new SubmitApplicationViewModel()
            {
                ApplicationId = applicationId,
                ApplicationReference = response.Reference,
                OrganisationId = response.OrganisationId,
                FormVersionId = response.FormVersionId,
                ApplicationName = response.Name
            });
        }

        [ValidateApplication]
        [HttpPost]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/submit")]
        public async Task<IActionResult> Submit(SubmitApplicationViewModel model)
        {
            var command = new SubmitApplicationCommand()
            {
                ApplicationId = model.ApplicationId,
                SubmittedBy = _userHelperService.GetUserDisplayName(),
                SubmittedByEmail = _userHelperService.GetUserEmail(),
            };
            await Send(command);

            return RedirectToAction(nameof(SubmitConfirmation), new { organisationId = _userHelperService.GetUserOrganisationId(), applicationId = model.ApplicationId });
        }

        [HttpGet]
        [ValidateApplication]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/submit-confirmed")]
        public async Task<IActionResult> SubmitConfirmation(Guid applicationId)
        {
            var query = new GetApplicationByIdQuery(applicationId);
            var response = await Send(query);
            return View(new SubmitApplicationViewModel()
            {
                ApplicationId = applicationId,
                ApplicationReference = response.Reference,
                OrganisationId = response.OrganisationId,
                FormVersionId = response.FormVersionId,
                ApplicationName = response.Name
            });
        }

        #endregion


        #region Preview
        [ValidateApplication]
        [HttpGet]
        [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/preview")]
        public async Task<IActionResult> ApplicationFormPreview(Guid organisationId, Guid applicationId, Guid formVersionId)
        {
            var query = new GetFormPreviewByIdQuery(applicationId);
            var response = await Send(query);

            var viewModel = ApplicationFormPreviewViewModel.Map(response, formVersionId, organisationId, applicationId);

            return View(viewModel);
        }
        #endregion
        private async Task HandleFileUploads(ApplicationPageViewModel viewModel)
        {
            foreach (var question in viewModel.Questions?.Where(q => q.Type == AODP.Models.Forms.QuestionType.File) ?? [])
            {
                foreach (var file in question.Answer?.FormFiles ?? [])
                {
                    using var stream = file.OpenReadStream();
                    await _fileService.UploadFileAsync($"{viewModel.ApplicationId}/{question.Id}", file.FileName, stream, file.ContentType);
                }
            }
        }

        private async Task DeleteApplicationFiles(Guid applicationId)
        {
            var files = _fileService.ListBlobs(applicationId.ToString());
            foreach (var file in files)
            {
                await _fileService.DeleteFileAsync(file.FullPath);
            }
        }
    }
}


