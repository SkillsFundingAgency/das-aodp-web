using SFA.DAS.AODP.Web.Models.TimelineComponents;

namespace SFA.DAS.AODP.Web.Models.Application;

public class ApplicationMessagesViewModel
{
    public List<ApplicationMessageViewModel> Messages { get; set; } = new();
}

public class ApplicationMessageViewModel : TimelineItemBase
{
    public string SentByEmail { get; set; }
}