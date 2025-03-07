namespace SFA.DAS.AODP.Web.Models.TimelineComponents;

public abstract class TimelineItemBase
{
    public Guid Id { get; set; }
    public string Status { get; set; } // to rename to messageHeader
    public string Text { get; set; }
    public DateTime SentAt { get; set; }
    public string SentByName { get; set; }
    public abstract string TimelineTitle { get; }
    public abstract string TimelineMetadata { get; }
    public abstract bool ShowText { get; }
}