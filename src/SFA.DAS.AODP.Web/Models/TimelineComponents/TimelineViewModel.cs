namespace SFA.DAS.AODP.Web.Models.TimelineComponents;

public class TimelineViewModel<T> where T : TimelineItemBase
{
    public List<T> Items { get; set; } = new();
}