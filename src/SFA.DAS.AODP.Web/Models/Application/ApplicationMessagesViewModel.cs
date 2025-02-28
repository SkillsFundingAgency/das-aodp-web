using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.TimelineComponents;
using System.Globalization;

namespace SFA.DAS.AODP.Web.Models.Application;

public class ApplicationMessagesViewModel
{
    public List<ApplicationMessageViewModel> Messages { get; set; } = new();
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