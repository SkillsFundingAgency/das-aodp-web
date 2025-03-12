using Microsoft.AspNetCore.Mvc.Rendering;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.TimelineComponents;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationMessage;

public class ApplicationMessagesViewModel
{
    public Guid ApplicationReviewId { get; set; }
    public Guid ApplicationId { get; set; }
    [Required]
    public string MessageText { get; set; }
    [Required]
    public string SelectedMessageType { get; set; }
    public string? SelectedMessageTypeDisplay => MessageTypeConfigurationRules.GetMessageSharingSettings(SelectedMessageType).DisplayName;
    public UserType UserType { get; set; }
    public List<SelectListItem> MessageTypeSelectOptions => MessageTypeSelectOptionRules.GetMessageTypeSelectOptions(UserType);
    public string? MessageInformationBanner => MessageTypeConfigurationRules.GetMessageSharingSettings(SelectedMessageType).MessageInformationBanner;
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


public static class MessageTypeSelectOptionRules
{
    public static readonly Dictionary<UserType, Func<List<SelectListItem>>> Options =
        new()
        {
            { UserType.Qfau, () => new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Choose message type" },
                    new SelectListItem { Value = "RequestInformationFromAOByQfau", Text = "Request Information From AO" },
                    new SelectListItem { Value = "UnlockApplication", Text = "Unlock Application" },
                    new SelectListItem { Value = "PutApplicationOnHold", Text = "Put Application On Hold" },
                    new SelectListItem { Value = "InternalNotes", Text = "Internal Notes" },
                    new SelectListItem { Value = "InternalNotesForPartners", Text = "Internal Notes For Partners" }
                }
            },
            { UserType.Ofqual, () => new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Choose message type" },
                    new SelectListItem { Value = "RequestInformationFromAOByOfqaul", Text = "Request Information From AO" },
                    new SelectListItem { Value = "InternalNotesForDfE", Text = "Internal Notes For DfE" }
                }
            },
            { UserType.SkillsEngland, () => new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Choose message type" },
                    new SelectListItem { Value = "RequestInformationFromAOBySkillsEngland", Text = "Request Information From AO" },
                    new SelectListItem { Value = "InternalNotesForDfE", Text = "Internal Notes For DfE" }
                }
            }
        };

    public static List<SelectListItem> GetMessageTypeSelectOptions(UserType userType)
    {
        if (Options.TryGetValue(userType, out var selectOptions))
        {
            return selectOptions();
        }

        return new List<SelectListItem> { };
    }
}