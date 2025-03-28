using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationMessage;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

[Area("Review")]
[Authorize(Policy = PolicyConstants.IsReviewUser)]
public class ApplicationMessagesController : ControllerBase
{
    public enum NotificationKeys { MessageSentBanner, MarkAsReadBanner }
    private readonly IUserHelperService _userHelperService;
    private readonly UserType UserType;
    public ApplicationMessagesController(IMediator mediator, ILogger<ApplicationMessagesController> logger, IUserHelperService userHelperService) : base(mediator, logger)
    {
        _userHelperService = userHelperService;
        UserType = userHelperService.GetUserType();
    }

    [HttpGet]
    [Route("review/{applicationReviewId}/messages")]
    public async Task<IActionResult> ApplicationMessagesAsync(Guid applicationReviewId)
    {
        var applicationId = await GetApplicationIdAsync(applicationReviewId);
        var response = await Send(new GetApplicationMessagesByIdQuery(applicationId, UserType.ToString()));
        var messages = response.Messages;

        var timelineMessages = new List<ApplicationMessageViewModel>();

        foreach (var message in messages)
        {
            timelineMessages.Add(new ApplicationMessageViewModel
            {
                Id = message.MessageId,
                Text = message.MessageText,
                MessageHeader = message.MessageHeader,
                SentAt = message.SentAt,
                SentByName = message.SentByName,
                SentByEmail = message.SentByEmail,
                UserType = UserType,
                MessageType = message.MessageType
            });
        }

        var model = new ApplicationMessagesViewModel
        {
            ApplicationReviewId = applicationReviewId,
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
        ShowNotificationIfKeyExists(NotificationKeys.MarkAsReadBanner.ToString(), ViewNotificationMessageType.Success, "All messages have been marked as read.");

        return View(model);
    }

    [HttpPost]
    [Route("review/{applicationReviewId}/messages")]
    public async Task<IActionResult> ApplicationMessages([FromForm] ApplicationMessagesViewModel model)
    {
        var applicationId = await GetApplicationIdAsync(model.ApplicationReviewId);

        if (!ModelState.IsValid)
        {
            model.AdditionalActions.Preview = false;
            model.AdditionalActions.Send = false;
            model.AdditionalActions.Edit = false;
            TempData.Remove("PreviewMessage");
            TempData.Remove("PreviewMessageType");
            TempData.Remove("EditMessage");
            model.UserType = UserType;
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
                    var messageId = await Send(new CreateApplicationMessageCommand(applicationId, model.MessageText, model.SelectedMessageType, UserType.ToString(), userEmail, userName));

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

            return RedirectToAction(nameof(ApplicationMessages), new { model.ApplicationReviewId });
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
        }
    }

    [HttpPost]
    [Route("review/{applicationReviewId}/messages/read")]
    public async Task<IActionResult> ReadApplicationMessages([FromForm] MarkApplicationMessagesAsReadViewModel model)
    {
        var applicationId = await GetApplicationIdAsync(model.ApplicationReviewId);

        await Send(new MarkAllMessagesAsReadCommand()
        {
            ApplicationId = applicationId,
            UserType = _userHelperService.GetUserType().ToString()
        });

        TempData[NotificationKeys.MarkAsReadBanner.ToString()] = true;
        return RedirectToAction(nameof(ApplicationMessages), new { model.ApplicationReviewId });

    }

    private async Task<Guid> GetApplicationIdAsync(Guid applicationReviewId)
    {
        var shared = await Send(new GetApplicationReviewSharingStatusByIdQuery(applicationReviewId));

        if (UserType == UserType.Ofqual || UserType == UserType.SkillsEngland)
        {
            if (UserType == UserType.Ofqual && !shared.SharedWithOfqual) throw new Exception("Application not shared with Ofqual.");
            if (UserType == UserType.SkillsEngland && !shared.SharedWithSkillsEngland) throw new Exception("Application not shared with Skills England.");
        }
        return shared.ApplicationId;
    }
}