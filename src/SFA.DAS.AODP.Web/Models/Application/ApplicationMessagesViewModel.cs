using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.TimelineComponents;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace SFA.DAS.AODP.Web.Models.Application;

public class ApplicationMessagesViewModel
{
    public Guid OrganisationId { get; set; }
    public Guid ApplicationId { get; set; }
    [Required]
    public string MessageText { get; set; }
    [Required]
    public string MessageType { get; set; }
    public string MessageTypeDisplay => MessageType switch
    {
        "RequestInformation" => "Request Information",
        "ReplyToInformationRequest" => "Reply To Information Request",
        _ => string.Empty,
    };

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
    public string SentByEmail { get; set; }
    public override string TimelineTitle
    {
        get
        {
            // if status is not null, display Status: ___, othwrwise just write out the message type
            return $"Status: {Status}"; // probably need a mapping of the enum to textual representation later
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
            return string.Equals(Status, "MessageSent", StringComparison.OrdinalIgnoreCase); // TODO: Confirm statuses to show Text for
        }
    }
}