
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
using SFA.DAS.AODP.Web.Areas.Apply.Storage;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationMessage;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Helpers.File;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.RelatedLinks;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

[Area("Review")]
//[Authorize(Policy = PolicyConstants.IsReviewUser)]
public class ApplicationMessagesController : ControllerBase
{
    public enum NotificationKeys { MessageSentBanner, MarkAsReadBanner }
    private readonly IUserHelperService _userHelperService;
    private readonly UserType UserType;
    private readonly FormBuilderSettings _formBuilderSettings;
    private readonly IMessageFileValidationService _messageFileValidationService;
    private readonly IFileService _blobFileService;
    private readonly FileUploadValidator _fileUploadValidator;

    public ApplicationMessagesController(IMediator mediator, ILogger<ApplicationMessagesController> logger, IUserHelperService userHelperService, FormBuilderSettings formBuilderSettings, IMessageFileValidationService messageFileValidationService, IFileService fileService, FileUploadValidator fileUploadValidator) : base(mediator, logger)
    {
        _userHelperService = userHelperService;
        UserType = userHelperService.GetUserType();
        _formBuilderSettings = formBuilderSettings;
        _messageFileValidationService = messageFileValidationService;
        _blobFileService = fileService;
        _fileUploadValidator = fileUploadValidator;
    }

    [HttpGet]
    [Route("review/{applicationReviewId}/messages", Name = RouteNames.Review_ApplicationMessages)]
    public async Task<IActionResult> ApplicationMessagesAsync(Guid applicationReviewId)
    {
        var applicationId = await GetApplicationIdWithAccessValidationAsync(applicationReviewId);
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
                        FileDisplayName = a.FileName,
                        FileId = a.FileId,
                        FormUrl = Url.Action(nameof(ApplicationReviewMessageFileDownload), "ApplicationMessages", new { applicationReviewId }),
                        IsDownloadable = a.IsDownloadable
                    }).ToList()
            });
        }

        var model = new ApplicationMessagesViewModel
        {
            ApplicationReviewId = applicationReviewId,
            TimelineMessages = timelineMessages,
            UserType = UserType,
        };
        model.SetLinks(Url, UserType, new RelatedLinksContext { ApplicationReviewId = applicationReviewId });

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

        ShowNotificationIfKeyExists(NotificationKeys.MessageSentBanner.ToString(), ViewNotificationMessageType.Success, "Your message has been sent.");
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
        model.SetLinks(Url, UserType, new RelatedLinksContext { ApplicationReviewId = model.ApplicationReviewId });

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
                    var response = await Send(new CreateApplicationMessageCommand(applicationId, model.MessageText, model.SelectedMessageType, UserType.ToString(), userEmail, userName));

                    if (model.Files.Count != 0)
                    {
                        try
                        {
                            await HandleFileUploadsAsync(applicationId, response.Id, model.Files);
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
    public async Task<IActionResult> ApplicationReviewMessageFileDownload(
        Guid applicationReviewId,
        Guid messageId,
        Guid fileId)
    {
        if (fileId == Guid.Empty)
            return BadRequest();

        var applicationId =
            await GetApplicationIdWithAccessValidationAsync(applicationReviewId);

        var message = await Send(new GetApplicationMessageByIdQuery(messageId));
        if (message == null)
            return NotFound();

        var userType = _userHelperService.GetUserType();
        var availableToUserType =
            (userType == UserType.Qfau && message.SharedWithDfe) ||
            (userType == UserType.SkillsEngland && message.SharedWithSkillsEngland) ||
            (userType == UserType.Ofqual && message.SharedWithOfqual);

        if (!availableToUserType)
            return Forbid();

        var fileMetadataResponse = await Send(new GetFileMetadataQuery
        {
            FileId = fileId
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

    private async Task HandleFileUploadsAsync(
     Guid applicationId,
     Guid messageId,
     List<IFormFile> files)
    {
        foreach (var file in files ?? [])
        {
            using var stream = file.OpenReadStream();

            _fileUploadValidator.ValidateOrThrow(
                file.FileName,
                stream);

            FileContext context = new FileContext
            (
                applicationId,
                null,
                messageId
            );

            var location = await _blobFileService.UploadAsync(
                FileCategory.MessageAttachment,
                context,
                file.FileName,
                file.ContentType,
                stream);

            await _mediator.Send(new CreateFileMetadataCommand
            {
                FileCategory = FileCategory.MessageAttachment,
                FileName = file.FileName,
                ContentType = file.ContentType,
                BlobContainer = location.Container,
                BlobPath = location.BlobPath,
                ApplicationId = applicationId,
                MessageId = messageId,
                UploadedBy = _userHelperService.GetUserDisplayName() ?? string.Empty,
            });
        }
    }
}