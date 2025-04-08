using SFA.DAS.AODP.Infrastructure.File;

namespace SFA.DAS.AODP.Web.Models.TimelineComponents;

public abstract class TimelineItemBase
{
    public Guid Id { get; set; }
    public string MessageHeader { get; set; }
    public string Text { get; set; }
    public DateTime SentAt { get; set; }
    public string SentByName { get; set; }
    public List<File> Files { get; set; } = new();

    public abstract string TimelineTitle { get; }
    public abstract string TimelineMetadata { get; }
    public abstract bool ShowText { get; }

    public class File
    {
        public required string FileDisplayName { get; init; }
        public required string FullPath { get; init; }
        public required string FormUrl { get; init; }
    }
}
