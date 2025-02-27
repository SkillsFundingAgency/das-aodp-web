using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Filters;
using SFA.DAS.AODP.Web.Models.Application;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Apply.Controllers;

[Area("Apply")]
[ValidateOrganisation]
public class ApplicationMessagesController : ControllerBase
{
    public ApplicationMessagesController(IMediator mediator, ILogger<ApplicationMessagesController> logger) : base(mediator, logger)
    {
    }

    [HttpGet]
    [Route("apply/organisations/{organisationId}/applications/{applicationId}/messages")]
    public IActionResult ApplicationMessages()
    {
        var model = new ApplicationMessagesViewModel
        {
            Messages = new List<ApplicationMessageViewModel>
            {
                new()
                {
                    Id = 1,
                    Text = "Dear Somebody, Please can you give further evidence of the application. Kind Regards, Davidowski",
                    Status = "MessageSent",
                    SentAt = DateTime.Now.AddMinutes(-30),
                    SentByName = "Davidowski",
                    SentByEmail = "test123@example.com"
                },
                new()
                {
                    Id = 2,
                    Text = "Test",
                    Status = "Awaiting Response From AO",
                    SentAt = DateTime.Now.AddMinutes(-20),
                    SentByName = "Admin",
                    SentByEmail = "admin@example.com"
                },
                new()
                {
                    Id = 3,
                    Text = "Test",
                    Status = "In Review",
                    SentAt = DateTime.Now.AddMinutes(-10),
                    SentByName = "AO",
                    SentByEmail = "ao@example.com"
                }
            }
        };

        return View(model);
    }
}