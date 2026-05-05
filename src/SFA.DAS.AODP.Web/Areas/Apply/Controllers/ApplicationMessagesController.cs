using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Aodp.Domain.Files;
using SFA.DAS.AODP.Application.Commands.Application.Application;
using SFA.DAS.AODP.Application.Commands.Files;
using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Application.Queries.Files.Get;
using SFA.DAS.AODP.Infrastructure.Common.IO;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Exceptions;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Apply.Models;
using SFA.DAS.AODP.Web.Areas.Apply.Storage;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Filters;
using SFA.DAS.AODP.Web.Helpers.File;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.RelatedLinks;
using System.Security.Cryptography.X509Certificates;
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
    private readonly IFileService _blobFileService;
    private readonly FileUploadValidator _fileUploadValidator;


    public ApplicationMessagesController(IMediator mediator, ILogger<ApplicationMessagesController> logger, IUserHelperService userHelperService, IMessageFileValidationService messageFileValidationService, FormBuilderSettings formBuilderSettings, IFileService blobService, FileUploadValidator fileUploadValidator) : base(mediator, logger)
    {
        _userHelperService = userHelperService;
        _messageFileValidationService = messageFileValidationService;
        _formBuilderSettings = formBuilderSettings;
        _blobFileService = blobService;
        _fileUploadValidator = fileUploadValidator;
    }

    [HttpGet]
    [Route("apply/organisations/{organisationId}/applications/{applicationId}/forms/{formVersionId}/messages", Name = RouteNames.Apply_ApplicationMessages)]
    public async Task<IActionResult> ApplicationMessages(Guid organisationId, Guid applicationId, Guid formVersionId)
    {
        var response = await Send(new GetApplicationMessagesByApplicationIdQuery(applicationId, UserType.ToString()));
        var messages = response.Messages;

        var timelineFilesResponse = await Send(new GetFileMetadataQuery
        {
            FileCategory = FileCategory.MessageAttachment,
            ApplicationId = applicationId
        });
        var timelineFiles = timelineFilesResponse.Files;

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
                Files = timelineFiles
                    .Where(t => t.MessageId == message.MessageId)
                    .Select(a => new ApplicationMessageViewModel.File()
                {
                    FileId = a.FileId,
                    FileDisplayName = a.FileName,
                    FormUrl = Url.Action(nameof(ApplicationMessageFileDownload), "ApplicationMessages", new { organisationId, applicationId, formVersionId }),
                    IsDownloadable = a.IsDownloadable,
                }).ToList()
            });
        }

        var model = new ApplicationMessagesViewModel
        {
            OrganisationId = organisationId,
            ApplicationId = applicationId,
            FormVersionId = formVersionId,
            TimelineMessages = timelineMessages,
            UserType = UserType
        };

        model.SetLinks(
            Url,
            UserType,
            new RelatedLinksContext
            {
                OrganisationId = organisationId,
                ApplicationId = applicationId,
                FormVersionId = formVersionId
            });

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

        model.SetLinks(
            Url,
            UserType,
            new RelatedLinksContext
            {
                OrganisationId = organisationId,
                ApplicationId = applicationId,
                FormVersionId = formVersionId
            });

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

        try
        {
            switch (true)
            {
                case var _ when model.AdditionalActions.Preview:
                    TempData["PreviewMessage"] = model.MessageText;
                    TempData.Remove("EditMessage");
                    break;

                case var _ when model.AdditionalActions.Send:

                    if (model.Files.Count != 0)
                    {
                        try
                        {
                            _messageFileValidationService.ValidateFiles(model.Files);
                        }
                        catch (FileUploadPolicyException ex)
                        {
                            _logger.LogError(ex, ex.Reason.ToUserMessage());
                            ModelState.AddModelError("Files", ex.Reason.ToUserMessage());
                            model.AdditionalActions.Preview = true;
                            return View(model);
                        }
                    }

                    string userEmail = _userHelperService.GetUserEmail();
                    string userName = _userHelperService.GetUserDisplayName();
                    var response = await Send(new CreateApplicationMessageCommand(model.ApplicationId, model.MessageText, model.SelectedMessageType, UserType.ToString(), userEmail, userName));

                    if (model.Files.Count != 0)
                    {
                        try
                        {
                            await HandleMessageAttachmentsUploadsAsync(applicationId, response.Id, model.Files);
                        }
                        catch (FileUploadPolicyException ex)
                        {
                            _logger.LogError(ex, ex.Reason.ToUserMessage());
                            ModelState.AddModelError("Files", ex.Reason.ToUserMessage());
                            model.AdditionalActions.Preview = true;
                            return View(model);
                        }
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
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View(model);
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
    public async Task<IActionResult> ApplicationMessageFileDownload(
        [FromRoute] Guid applicationId,
        [FromForm] Guid messageId,
        [FromForm] Guid fileId)
    {
        if (fileId == Guid.Empty)
            return BadRequest();

        var message = await Send(new GetApplicationMessageByIdQuery(messageId));
        if (message == null || !message.SharedWithAwardingOrganisation)
            return Forbid();

        var fileMetadataResponse = await Send(new GetFileMetadataQuery
        {
            FileId = fileId,
        });

        var file = fileMetadataResponse?.Files?.SingleOrDefault();

        if (file == null)
            return NotFound();

        if (file.ApplicationId != applicationId || file.MessageId != messageId)
            return Forbid();

        if (!file.IsDownloadable)
            return Forbid();

        var stream = await _blobFileService.OpenReadStreamAsync(
            file.BlobContainer,
            file.BlobPath);

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType;

        return File(stream, contentType, file.FileName);
    }

    private async Task HandleMessageAttachmentsUploadsAsync(
        Guid applicationId,
        Guid messageId,
        List<IFormFile> files)
    {
        foreach (var file in files ?? [])
        {
            using var stream = file.OpenReadStream();


            _fileUploadValidator.ValidateOrThrow(
                    file.FileName,
                    stream,
                    importFileSize: null);

            var location = await _blobFileService.UploadAsync(
                FileCategory.MessageAttachment,
                new (applicationId,null,messageId),
                file.FileName,
                file.ContentType,
                stream
                );

            await _mediator.Send(new CreateFileMetadataCommand
            {
                FileCategory = FileCategory.MessageAttachment,
                FileName = file.Name,
                ContentType = file.ContentType,
                BlobPath = location.BlobPath,
                BlobContainer = location.Container,
                ApplicationId = applicationId,
                MessageId = messageId,
                UploadedBy = _userHelperService.GetUserDisplayName() ?? string.Empty,
            });
        }
    } 
}