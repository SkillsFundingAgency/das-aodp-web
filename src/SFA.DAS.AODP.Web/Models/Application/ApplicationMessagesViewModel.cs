namespace SFA.DAS.AODP.Web.Models.Application;

public class ApplicationMessagesViewModel
{
    public List<ApplicationMessageViewModel> Messages { get; set; } = new();
}

public class ApplicationMessageViewModel
{
    public int Id { get; set; }
    public string Text { get; set; }
    public string Status { get; set; }
    public string Type { get; set; }
    public DateTime SentAt { get; set; }
    public string SentByName { get; set; }
    public string SentByEmail { get; set; }
}