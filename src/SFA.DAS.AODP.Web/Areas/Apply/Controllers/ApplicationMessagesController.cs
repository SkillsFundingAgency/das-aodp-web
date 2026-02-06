using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Apply.Models;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Filters;
using SFA.DAS.AODP.Web.Helpers.File;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.RelatedLinks;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Apply.Controllers;

[Area("Apply")]
[Authorize(Policy = PolicyConstants.IsApplyUser)]
[ValidateOrganisation]
[ValidateApplication]
public class ApplicationMessagesController : ControllerBase
{
    public enum NotificationKeys { MessageSentBanner, MessageVisibilityBanner, MarkAsReadBanner }
    private readonly IUserHelperService _userHelperService;
    private readonly UserType UserType = UserType.AwardingOrganisation;
    private readonly IMessageFileValidationService _messageFileValidationService;
    private readonly FormBuilderSettings _formBuilderSettings;
    private readonly IFileService _fileService;


    public ApplicationMessagesController(IMediator mediator, ILogger<ApplicationMessagesController> logger, IUserHelperService userHelperService, IMessageFileValidationService messageFileValidationService, FormBuilderSettings formBuilderSettings, IFileService fileService) : base(mediator, logger)
    {
        _userHelperService = userHelperService;
        _messageFileValidationService = messageFileValidationService;
        _formBuilderSettings = formBuilderSettings;
        _fileService = fileService;
    }

    [HttpGet]
    [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/messages")]
    public async Task<IActionResult> ApplicationMessages(Guid organisationId, Guid applicationId, Guid formVersionId)
    {
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
                    FileDisplayName = a.FileName,
                    FullPath = a.FullPath,
                    FormUrl = Url.Action(nameof(ApplicationMessageFileDownload), "ApplicationMessages", new { organisationId, applicationId, formVersionId })
                }).ToList()
            });
        }

        var model = new ApplicationMessagesViewModel
        {
            OrganisationId = organisationId,
            ApplicationId = applicationId,
            FormVersionId = formVersionId,
            TimelineMessages = timelineMessages,
            UserType = UserType,
            RelatedLinks = RelatedLinksBuilder.Build(
                Url,
                RelatedLinksPage.ApplyApplicationMessages,
                UserType,
                new RelatedLinksContext
                {
                    OrganisationId = organisationId,
                    ApplicationId = applicationId,
                    FormVersionId = formVersionId
                })
        };

        if (TempData.ContainsKey("EditMessage"))
        {
            model.MessageText = TempData.Peek("EditMessage")?.ToString();
            model.AdditionalActions.Preview = false;
        }
        else if (TempData.ContainsKey("PreviewMessage"))
        {
            model.MessageText = TempData["PreviewMessage"]?.ToString();
            model.AdditionalActions.Preview = true;
        }
        else
        {
            model.AdditionalActions.Preview = false;
        }

        ShowNotificationIfKeyExists(NotificationKeys.MessageSentBanner.ToString(), ViewNotificationMessageType.Success, "Your message has been sent.");
        ShowNotificationIfKeyExists(NotificationKeys.MarkAsReadBanner.ToString(), ViewNotificationMessageType.Success, "All messages have been marked as read.");

        model.FileSettings = _formBuilderSettings;
        return View(model);
    }

    [HttpPost]
    [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/messages")]
    public async Task<IActionResult> ApplicationMessages([FromForm] ApplicationMessagesViewModel model, [FromRoute] Guid applicationId, [FromRoute] Guid organisationId, [FromRoute] Guid formVersionId)
    {
        model.ApplicationId = applicationId;
        model.OrganisationId = organisationId;
        model.FormVersionId = formVersionId;

        model.FileSettings = _formBuilderSettings;
        if (!ModelState.IsValid)
        {
            model.AdditionalActions.Preview = false;
            model.AdditionalActions.Send = false;
            model.AdditionalActions.Edit = false;
            TempData.Remove("PreviewMessage");
            TempData.Remove("EditMessage");
            return View(model);
        }
        switch (true)
        {
            case var _ when model.AdditionalActions.Preview:
                TempData["PreviewMessage"] = model.MessageText;
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
                var response = await Send(new CreateApplicationMessageCommand(model.ApplicationId, model.MessageText, model.SelectedMessageType, UserType.ToString(), userEmail, userName));

                if (model.Files != null && model.Files.Count != 0)
                {
                    await HandleFileUploadsAsync(model.ApplicationId, response.Id, model.Files);
                }

                if (response?.EmailSent ?? false)
                {
                    TempData[NotificationKeys.MessageSentBanner.ToString()] = true;
                }
                else
                {
                    TempData.Remove(NotificationKeys.MessageSentBanner.ToString());
                }

                TempData.Remove("PreviewMessage");
                TempData.Remove("EditMessage");
                break;

            case var _ when model.AdditionalActions.Edit:
                TempData["EditMessage"] = model.MessageText;
                TempData["EditMessageType"] = model.SelectedMessageType;
                break;
        }

        return RedirectToAction(nameof(ApplicationMessages), new { model.OrganisationId, model.ApplicationId, model.FormVersionId });
    }

    [HttpPost]
    [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/messages/read")]
    public async Task<IActionResult> ReadApplicationMessages(MarkApplicationMessagesAsReadViewModel model, [FromRoute] Guid applicationId, [FromRoute] Guid organisationId, [FromRoute] Guid formVersionId)
    {
        model.ApplicationId = applicationId;
        model.OrganisationId = organisationId;
        model.FormVersionId = formVersionId;

        await Send(new MarkAllMessagesAsReadCommand()
        {
            ApplicationId = model.ApplicationId,
            UserType = UserType.AwardingOrganisation.ToString()
        });

        TempData[NotificationKeys.MarkAsReadBanner.ToString()] = true;
        return RedirectToAction(nameof(ApplicationMessages), new { model.OrganisationId, model.ApplicationId, model.FormVersionId });
    }


    [HttpPost]
    [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/message-file-download")]
    public async Task<IActionResult> ApplicationMessageFileDownload([FromForm] string filePath, [FromRoute] Guid applicationId, [FromForm] Guid messageId)
    {
        // Ensure file path application id matches the route application id and the form message id
        // The [ValidateApplication] filter ensures the user has access to the application id in the route
        if (!filePath.StartsWith($"messages/{applicationId}/{messageId}/"))
        {
            return BadRequest();
        }

        // Now check whether the user has access to this particular message
        var message = await Send(new GetApplicationMessageByIdQuery(messageId));
        if (message == null || !message.SharedWithAwardingOrganisation) return BadRequest();

        var file = await _fileService.GetBlobDetails(filePath.ToString());
        var fileStream = await _fileService.OpenReadStreamAsync(filePath);
        return File(fileStream, "application/octet-stream", file.FileName);
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