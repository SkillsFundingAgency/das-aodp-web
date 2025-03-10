using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationMessage;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

[Area("Review")]
public class ApplicationMessagesController : ControllerBase
{
    public enum NotificationKeys { MessageSentBanner }
    private readonly IUserHelperService _userHelperService;
    private readonly UserType UserType;
    public ApplicationMessagesController(IMediator mediator, ILogger<ApplicationMessagesController> logger, IUserHelperService userHelperService) : base(mediator, logger)
    {
        _userHelperService = userHelperService;
        UserType = userHelperService.GetUserType();
    }

    [HttpGet]
    [Route("review/{applicationReviewId}/applications/{applicationId}/messages")]
    public async Task<IActionResult> ApplicationMessagesAsync(Guid applicationReviewId, Guid applicationId)
    {
        var response = await Send(new GetApplicationMessagesByIdQuery(applicationId, UserType.ToString()));
        var messages = response.Messages;

        var timelineMessages = new List<ApplicationMessageViewModel>();

        foreach (var message in messages)
        {
            timelineMessages.Add(new ApplicationMessageViewModel
            {
                Id = message.MessageId,
                Text = message.MessageText,
                Status = message.MessageHeader,
                SentAt = message.SentAt,
                SentByName = message.SentByName,
                SentByEmail = message.SentByEmail,
                UserType = UserType
            });
        }

        var model = new ApplicationMessagesViewModel
        {
            ApplicationReviewId = applicationReviewId,
            ApplicationId = applicationId,
            TimelineMessages = timelineMessages,
            UserType = UserType
        };

        if (TempData.ContainsKey("EditMessage"))
        {
            model.MessageText = TempData.Peek("EditMessage")?.ToString();
            model.SelectedMessageType = TempData.Peek("EditMessageType")?.ToString();
            model.AdditionalActions.Preview = false;
        }
        else if (TempData.ContainsKey("PreviewMessage"))
        {
            model.MessageText = TempData["PreviewMessage"]?.ToString();
            model.SelectedMessageType = TempData["PreviewMessageType"]?.ToString();
            model.AdditionalActions.Preview = true;
        }
        else
        {
            model.AdditionalActions.Preview = false;
        }

        ShowNotificationIfKeyExists(NotificationKeys.MessageSentBanner.ToString(), ViewNotificationMessageType.Success, "Your message has been sent");

        return View(model);
    }

    [HttpPost]
    [Route("review/{applicationReviewId}/applications/{applicationId}/messages")]
    public async Task<IActionResult> ApplicationMessages([FromForm] ApplicationMessagesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AdditionalActions.Preview = false;
            model.AdditionalActions.Send = false;
            model.AdditionalActions.Edit = false;
            TempData.Remove("PreviewMessage");
            TempData.Remove("PreviewMessageType");
            TempData.Remove("EditMessage");
            return View(model);
        }

        try
        {
            switch (true)
            {
                case var _ when model.AdditionalActions.Preview:
                    TempData["PreviewMessage"] = model.MessageText;
                    TempData["PreviewMessageType"] = model.SelectedMessageType;
                    TempData.Remove("EditMessage");
                    break;

                case var _ when model.AdditionalActions.Send:
                    string userEmail = _userHelperService.GetUserEmail().ToString();
                    string userName = _userHelperService.GetUserDisplayName().ToString();
                    string messageTypeToSend = SendMessageTypeRule.GetMessageType(_userHelperService.GetUserType(), model.SelectedMessageType);
                    var messageId = await Send(new CreateApplicationMessageCommand(model.ApplicationId, model.MessageText, messageTypeToSend, UserType.ToString(), userEmail, userName));

                    TempData[NotificationKeys.MessageSentBanner.ToString()] = "Your message has been sent";
                    TempData.Remove("PreviewMessage");
                    TempData.Remove("PreviewMessageType");
                    TempData.Remove("EditMessage");
                    break;

                case var _ when model.AdditionalActions.Edit:
                    TempData["EditMessage"] = model.MessageText;
                    TempData["EditMessageType"] = model.SelectedMessageType;
                    break;
            }

            return RedirectToAction(nameof(ApplicationMessages), new { model.ApplicationReviewId, model.ApplicationId });
        }
        catch
        {
            return View(model);
        }
    }
}