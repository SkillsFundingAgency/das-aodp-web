namespace SFA.DAS.AODP.Web.Models.TimelineComponents;

public abstract class TimelineItemBase
{
    public int Id { get; set; }
    public string Status { get; set; }
    public string Text { get; set; }
    public DateTime SentAt { get; set; }
    public string SentByName { get; set; }
}