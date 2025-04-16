using Azure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Models;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationMessage;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.File;
using SFA.DAS.AODP.Web.Helpers.User;
using StackExchange.Redis;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

[Area("Review")]
[Authorize(Policy = PolicyConstants.IsReviewUser)]
public class ApplicationMessagesController : ControllerBase
{
    public enum NotificationKeys { MessageSentBanner, MarkAsReadBanner }
    private readonly IUserHelperService _userHelperService;
    private readonly UserType UserType;
    private readonly FormBuilderSettings _formBuilderSettings;
    private readonly IMessageFileValidationService _messageFileValidationService;
    private readonly IFileService _fileService;

    public ApplicationMessagesController(IMediator mediator, ILogger<ApplicationMessagesController> logger, IUserHelperService userHelperService, FormBuilderSettings formBuilderSettings, IMessageFileValidationService messageFileValidationService, IFileService fileService) : base(mediator, logger)
    {
        _userHelperService = userHelperService;
        UserType = userHelperService.GetUserType();
        _formBuilderSettings = formBuilderSettings;
        _messageFileValidationService = messageFileValidationService;
        _fileService = fileService;
    }

    [HttpGet]
    [Route("review/{applicationReviewId}/messages")]
    public async Task<IActionResult> ApplicationMessagesAsync(Guid applicationReviewId)
    {
        var applicationId = await GetApplicationIdWithAccessValidationAsync(applicationReviewId);
        var response = await Send(new GetApplicationMessagesByApplicationIdQuery(applicationId, UserType.ToString()));
        var messages = response.Messages;

        var timelineFiles = await GetApplicationMessageFilesAsync(applicationId);
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
                MessageType = message.MessageType,
                Files = timelineFiles.Where(t => t.FullPath.StartsWith($"messages/{applicationId}/{message.MessageId}")).Select(a => new ApplicationMessageViewModel.File()
                {
                    FileDisplayName = a.FileNameWithPrefix,
                    FullPath = a.FullPath,
                    FormUrl = Url.Action(nameof(ApplicationReviewMessageFileDownload), "ApplicationMessages", new { applicationReviewId })
                }).ToList()
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

        ShowNotificationIfKeyExists(NotificationKeys.MessageSentBanner.ToString(), ViewNotificationMessageType.Success, "Your message has been logged but no notification will be sent.");
        ShowNotificationIfKeyExists(NotificationKeys.MarkAsReadBanner.ToString(), ViewNotificationMessageType.Success, "All messages have been marked as read.");

        model.FileSettings = _formBuilderSettings;
        return View(model);
    }

    [HttpPost]
    [Route("review/{applicationReviewId}/messages")]
    public async Task<IActionResult> ApplicationMessages([FromForm] ApplicationMessagesViewModel model)
    {
        model.FileSettings = _formBuilderSettings;
        var applicationId = await GetApplicationIdWithAccessValidationAsync(model.ApplicationReviewId);

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
                    if (model.Files != null && model.Files.Count != 0)
                    {
                        try
                        {
                            _messageFileValidationService.ValidateFiles(model.Files);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error validating message files");
                            ModelState.AddModelError("Files", $"The files validation was not successful: {ex.Message}");
                            model.AdditionalActions.Preview = true;
                            return View(model);
                        }
                    }
                    string userEmail = _userHelperService.GetUserEmail();
                    string userName = _userHelperService.GetUserDisplayName();
                    var response = await Send(new CreateApplicationMessageCommand(applicationId, model.MessageText, model.SelectedMessageType, UserType.ToString(), userEmail, userName));

                    if (model.Files != null && model.Files.Count != 0)
                    {
                        await HandleFileUploadsAsync(applicationId, response.Id, model.Files);
                    }

                    TempData[NotificationKeys.MessageSentBanner.ToString()] = true;
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
        var applicationId = await GetApplicationIdWithAccessValidationAsync(model.ApplicationReviewId);

        await Send(new MarkAllMessagesAsReadCommand()
        {
            ApplicationId = applicationId,
            UserType = _userHelperService.GetUserType().ToString()
        });

        TempData[NotificationKeys.MarkAsReadBanner.ToString()] = true;
        return RedirectToAction(nameof(ApplicationMessages), new { model.ApplicationReviewId });
    }


    [HttpPost]
    [Route("review/{applicationReviewId}/message-file-download")]
    public async Task<IActionResult> ApplicationReviewMessageFileDownload([FromForm] string filePath, [FromRoute] Guid applicationReviewId, [FromForm] Guid messageId)
    {
        var applicationId = await GetApplicationIdWithAccessValidationAsync(applicationReviewId);

        if (!filePath.StartsWith($"messages/{applicationId}/{messageId}/"))
        {
            return BadRequest();
        }

        // Now check whether the user has access to this particular message
        var message = await Send(new GetApplicationMessageByIdQuery(messageId));
        if (message == null) return BadRequest();

        var availableToUserType = false;
        var userType = _userHelperService.GetUserType();
        if ((userType == UserType.Qfau && message.SharedWithDfe) ||
              (userType == UserType.SkillsEngland && message.SharedWithSkillsEngland) ||
              (userType == UserType.Ofqual && message.SharedWithOfqual))
        {
            availableToUserType = true;
        }

        if (!availableToUserType) return BadRequest();


        var file = await _fileService.GetBlobDetails(filePath.ToString());
        var fileStream = await _fileService.OpenReadStreamAsync(filePath);
        return File(fileStream, "application/octet-stream", file.FileNameWithPrefix);
    }

    private async Task<Guid> GetApplicationIdWithAccessValidationAsync(Guid applicationReviewId)
    {
        var shared = await Send(new GetApplicationReviewSharingStatusByIdQuery(applicationReviewId));

        if (UserType == UserType.Ofqual || UserType == UserType.SkillsEngland)
        {
            if (UserType == UserType.Ofqual && !shared.SharedWithOfqual) throw new Exception("Application not shared with Ofqual.");
            if (UserType == UserType.SkillsEngland && !shared.SharedWithSkillsEngland) throw new Exception("Application not shared with Skills England.");
        }
        return shared.ApplicationId;
    }

    private async Task HandleFileUploadsAsync(Guid applicationId, Guid messageId, List<IFormFile> files)
    {
        var metadata = await Send(new GetApplicationMetadataByIdQuery(applicationId));

        foreach (var file in files ?? [])
        {
            using var stream = file.OpenReadStream();
            await _fileService.UploadFileAsync($"messages/{applicationId}/{messageId}", file.FileName, stream, file.ContentType, metadata.Reference.ToString().PadLeft(6, '0'));
        }
    }

    private async Task<List<UploadedBlob>> GetApplicationMessageFilesAsync(Guid applicationId)
    {
        return _fileService.ListBlobs($"messages/{applicationId}");
    }
}