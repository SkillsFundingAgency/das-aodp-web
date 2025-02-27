using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Web.Filters;
using SFA.DAS.AODP.Web.Models.Application;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Apply.Controllers;

[ValidateOrganisation]
public class ApplicationMessagesController : ControllerBase
{
    public ApplicationMessagesController(IMediator mediator, ILogger<ApplicationMessagesController> logger) : base(mediator, logger)
    {
    }

    [HttpGet]
    [Route("organisations/{organisationId}/applications/{applicationId}/messages")]
    public IActionResult ApplicationMessages()
    {
        var model = new ApplicationMessagesViewModel
        {
            Messages = new List<ApplicationMessageViewModel>
            {
                new()
                {
                    Id = 1,
                    Text = "Application submitted successfully.",
                    Status = "Submitted",
                    Type = "Info",
                    SentAt = DateTime.Now.AddMinutes(-30),
                    SentByName = "John Doe",
                    SentByEmail = "john.doe@example.com"
                },
                new()
                {
                    Id = 2,
                    Text = "Application is under review.",
                    Status = "In Progress",
                    Type = "Update",
                    SentAt = DateTime.Now.AddMinutes(-20),
                    SentByName = "Admin",
                    SentByEmail = "admin@example.com"
                },
                new()
                {
                    Id = 3,
                    Text = "Application approved!",
                    Status = "Approved",
                    Type = "Success",
                    SentAt = DateTime.Now.AddMinutes(-10),
                    SentByName = "Manager",
                    SentByEmail = "manager@example.com"
                }
            }
        };

        return View(model);
    }
}