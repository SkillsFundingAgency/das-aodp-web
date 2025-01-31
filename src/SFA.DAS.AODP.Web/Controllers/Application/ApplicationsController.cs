using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Models.Application;

namespace SFA.DAS.AODP.Web.Controllers.Application
{
    public class ApplicationsController : Controller
    {
        [HttpGet]
        [Route("organisations/{organisationId}")]
        public IActionResult Index(Guid organisationId)
        {
            return View(new ListApplicationsViewModel()
            {
                OrganisationId = Guid.NewGuid(),
                Applications = new()
                 {
                     new()
                     {
                          CreatedDate = DateTime.Now,
                           Id = Guid.NewGuid(),
                            LastSubmittedDate = DateTime.Now,
                             Name = "name",
                              Owner  = "owner",
                               Reference = "RF123",
                                Submitted = false
                     }
                 }
            });
        }

        [HttpGet]
        [Route("organisations/{organisationId}/forms")]
        public IActionResult AvailableForms(Guid organisationId)
        {
            return View(new ListAvailableFormsViewModel()
            {
                OrganisationId = organisationId,
                Forms = new()
                {
                    new()
                    {
                         Id = Guid.NewGuid(),
                         Title = "name",
                          Description = "description",
                          Order = 1,
                    }
                }
            });
        }

        [HttpGet]
        [Route("organisations/{organisationId}/forms/{formVersionId}/Create")]
        public IActionResult Create(Guid organisationId, Guid formVersionId)
        {
            return View(new CreateApplicationViewModel()
            {
                OrganisationId = organisationId,
                FormVersionId = formVersionId,
                FormTitle = "Some form"
            });
        }


        [HttpPost]
        [Route("organisations/{organisationId}/forms/{formVersionId}/Create")]
        public IActionResult Create(CreateApplicationViewModel createApplicationViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(createApplicationViewModel);
            }

            return NotFound();
        }


        [HttpGet]
        [Route("organisations/{organisationId}/applications/{applicationId}")]
        public IActionResult ViewApplication(Guid organisationId, Guid applicationId)
        {
            return View(new ApplicationFormViewModel()
            {
                OrganisationId = organisationId,
                ApplicationName = "Application name",
                Owner = "owenr",
                FormVersionId = Guid.NewGuid(),
                IsCompleted = true,
                IsSubmitted = true,
                Sections = new()
                {
                          new()   {
                               Id = Guid.NewGuid(),
                                Description = "description",
                                Order = 1,
                                RemainingMandatoryQuestions = 1,
                                Title = "title",
                          }
                }

            });
        }


        [HttpGet]
        [Route("organisations/{organisationId}/applications/{applicationId}/section/{sectionId}")]
        public IActionResult ViewApplicationSection(Guid organisationId, Guid applicationId, Guid sectionId)
        {
            return View(new ApplicationSectionViewModel()
            {
                OrganisationId = organisationId,
                ApplicationName = "Application name",
                FormVersionId = Guid.NewGuid(),
                IsCompleted = true,
                IsSubmitted = true,
                Pages = new()
                {
                          new()   {
                               Id = Guid.NewGuid(),
                                Order = 1,
                                Title = "title",
                          }
                }

            });
        }
    }
}
