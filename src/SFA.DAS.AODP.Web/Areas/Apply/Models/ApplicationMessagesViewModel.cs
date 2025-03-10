using Microsoft.AspNetCore.Mvc.Rendering;
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
    [Required]
    public string MessageText { get; set; }
    [Required]
    public string SelectedMessageType => "ReplyToInformationRequest";
    public string SelectedMessageTypeDisplay => MessageTypeConfigurationRules.GetMessageSharingSettings(SelectedMessageType).DisplayName;
    public UserType UserType { get; set; }
    public List<SelectListItem> MessageTypeSelectOptions => MessageTypeSelectOptionRules.GetMessageTypeSelectOptions(UserType);
    public List<ApplicationMessageViewModel>? TimelineMessages { get; set; } = new();
    public MessageActions AdditionalActions { get; set; } = new();

    public class MessageActions
    {
        public bool Preview { get; set; }
        public bool Send { get; set; }
        public bool Edit { get; set; }
    }
}

public class ApplicationMessageViewModel : TimelineItemBase
{
    public string MessageType { get; set; }
    public UserType UserType { get; set; }
    public MessageTypeConfiguration MessageTypeConfiguration => MessageTypeConfigurationRules.GetMessageSharingSettings(MessageType);
    public bool VisibleToUser => MessageTypeConfiguration.AvailableTo.Contains(UserType);
    public string MessageTypeDisplay => MessageTypeConfiguration.DisplayName;
    public string SentByEmail { get; set; }
    public override string TimelineTitle
    {
        get
        {
            return (Status == null) ? $"{Status}" : $"Status: {Status}";
        }
    }
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