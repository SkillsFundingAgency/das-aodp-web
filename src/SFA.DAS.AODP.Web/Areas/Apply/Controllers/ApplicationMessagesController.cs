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
    public IActionResult ApplicationMessages(Guid organisationId, Guid applicationId)
    {
        var model = new ApplicationMessagesViewModel
        {
            OrganisationId = organisationId,
            ApplicationId = applicationId,
            TimelineMessages = new List<ApplicationMessageViewModel>
                    {
                        new ApplicationMessageViewModel
                        {
                            Id = 1,
                            Text = "Dear Somebody, Please can you give further evidence of the application. Kind Regards, Davidowski",
                            Status = "MessageSent",
                            SentAt = DateTime.Now.AddMinutes(-30),
                            SentByName = "Davidowski",
                            SentByEmail = "test123@example.com"
                        },
                        new ApplicationMessageViewModel
                        {
                            Id = 2,
                            Text = "Test",
                            Status = "Awaiting Response From AO",
                            SentAt = DateTime.Now.AddMinutes(-20),
                            SentByName = "Admin",
                            SentByEmail = "admin@example.com"
                        },
                        new ApplicationMessageViewModel
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

        if (TempData.ContainsKey("PreviewMessage"))
        {
            model.MessageText = TempData["PreviewMessage"]?.ToString();
            model.MessageType = TempData["PreviewMessageType"]?.ToString();
            model.AdditionalActions.Preview = true;
        }
        else
        {
            model.AdditionalActions.Preview = false;
        }

        if (TempData.ContainsKey("SuccessBanner"))
        {
            ViewBag.SuccessBanner = TempData["SuccessBanner"];
        }

        return View(model);
    }

    [HttpPost]
    [Route("apply/organisations/{organisationId}/applications/{applicationId}/messages")]
    public async Task<IActionResult> ApplicationMessages([FromForm] ApplicationMessagesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AdditionalActions.Preview = false;
            model.AdditionalActions.Send = false;
            TempData.Remove("PreviewMessage");
            return View(model);
        }

        try
        {
            if (model.AdditionalActions.Preview)
            {
                TempData["PreviewMessage"] = model.MessageText;
                TempData["PreviewMessageType"] = model.MessageType;
            }
            else if (model.AdditionalActions.Send)
            {
                //var messageId = await Send(new CreateApplicationMessageCommand(model.MessageText, model.ApplicationId));
                TempData["SuccessBanner"] = "Message sent successfully!";
                TempData.Remove("PreviewMessage");
            }

            return RedirectToAction(nameof(ApplicationMessages), new { model.OrganisationId, model.ApplicationId });
        }
        catch
        {
            return View(model);
        }
    }
}