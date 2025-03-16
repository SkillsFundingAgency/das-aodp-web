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
    public string Hint => (UserType == UserType.Qfau) ?
        "Leave messages, comments and recommendations to other DfE staff members, IfATE, Ofqual or the AO applicant owner."
        : "Leave messages, comments and recommendations to DfE or the AO applicant owner.";
    [Required]
    public string MessageText { get; set; }
    [Required]
    public string SelectedMessageType { get; set; }
    public MessageTypeConfiguration SelectedMessageTypeConfiguration => MessageTypeConfigurationRules.GetMessageSharingSettings(SelectedMessageType);
    public string? SelectedMessageTypeDisplay => SelectedMessageTypeConfiguration.DisplayName;
    public UserType UserType { get; set; }
    public List<SelectListItem> MessageTypeSelectOptions => MessageTypeSelectOptionRules.GetMessageTypeSelectOptions(UserType);
    public string? MessageInformationBanner => SelectedMessageTypeConfiguration.MessageInformationBanner;
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


public static class MessageTypeSelectOptionRules
{
    public static readonly Dictionary<UserType, Func<List<SelectListItem>>> Options =
        new()
        {
            { UserType.Qfau, () => new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Choose message type" },
                    new SelectListItem { Value = MessageType.RequestInformationFromAOByQfau.ToString() , Text = "Request information from AO" },
                    new SelectListItem { Value = MessageType.UnlockApplication.ToString(), Text = "Unlock application" },
                    new SelectListItem { Value = MessageType.PutApplicationOnHold.ToString(), Text = "Put application on hold" },
                    new SelectListItem { Value = MessageType.InternalNotes.ToString(), Text = "Internal notes" },
                    new SelectListItem { Value = MessageType.InternalNotesForPartners.ToString(), Text = "Internal notes for partners" }
                }
            },
            { UserType.Ofqual, () => new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Choose message type" },
                    new SelectListItem { Value = MessageType.RequestInformationFromAOByOfqaul.ToString(), Text = "Request information from AO" },
                    new SelectListItem { Value = MessageType.InternalNotesForQfauFromOfqual.ToString(), Text = "Internal notes for DfE" }
                }
            },
            { UserType.SkillsEngland, () => new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Choose message type" },
                    new SelectListItem { Value = MessageType.RequestInformationFromAOBySkillsEngland.ToString(), Text = "Request information from AO" },
                    new SelectListItem { Value = MessageType.InternalNotesForQfauFromSkillsEngland.ToString(), Text = "Internal notes for DfE" }
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