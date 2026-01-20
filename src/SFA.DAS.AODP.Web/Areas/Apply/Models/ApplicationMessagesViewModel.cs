using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationMessage;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.TimelineComponents;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SFA.DAS.AODP.Web.Areas.Apply.Models;
public class ApplicationMessagesViewModel
{
    public Guid OrganisationId { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid FormVersionId { get; set; }

    [Required(ErrorMessage = "Enter some text in the message field.")]
    public string MessageText { get; set; }
    [Required]
    public string SelectedMessageType => MessageType.ReplyToInformationRequest.ToString();
    public string SelectedMessageTypeDisplay => MessageTypeConfigurationRules.GetMessageSharingSettings(SelectedMessageType).DisplayName;

    public List<IFormFile> Files { get; set; } = new();

    public UserType UserType { get; set; }
    public List<SelectListItem> MessageTypeSelectOptions => MessageTypeSelectOptionRules.GetMessageTypeSelectOptions(UserType);
    public List<ApplicationMessageViewModel>? TimelineMessages { get; set; } = new();
    public MessageActions AdditionalActions { get; set; } = new();
    public FileUploadSetting? FileSettings { get; set; }

    public class MessageActions
    {
        public bool Preview { get; set; }
        public bool Send { get; set; }
        public bool Edit { get; set; }
    }

    public class FileUploadSetting
    {
        public List<string> UploadFileTypesAllowed { get; set; }
        public int MaxUploadFileSize { get; set; }
        public int MaxUploadNumberOfFiles { get; set; }

        public static implicit operator FileUploadSetting(FormBuilderSettings formBuilderSettings)
        {
            return new()
            {
                UploadFileTypesAllowed = formBuilderSettings.UploadFileTypesAllowed,
                MaxUploadFileSize = formBuilderSettings.MaxUploadFileSize,
                MaxUploadNumberOfFiles = formBuilderSettings.MaxUploadNumberOfFiles,
            };
        }
    }
}

public class ApplicationMessageViewModel : TimelineItemBase
{
    public string MessageType { get; set; }
    public UserType UserType { get; set; }
    public MessageTypeConfiguration MessageTypeConfiguration => MessageTypeConfigurationRules.GetMessageSharingSettings(MessageType);
    public string MessageTypeDisplay => MessageTypeConfiguration.DisplayName;
    public string SentByEmail { get; set; }
    public override string TimelineTitle => $"{MessageHeader}";
    public override string TimelineMetadata
    {
        get
        {
            return $"{SentByName}, {SentAt.ToString("dd MMM yyyy 'at' HH:mm", CultureInfo.InvariantCulture)}";
        }
    }
    public override bool ShowText
    {
        get
        {
            return (Text.Length > 0) ? true : false;
        }
    }
}