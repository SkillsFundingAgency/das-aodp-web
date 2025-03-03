namespace SFA.DAS.AODP.Models.Application;

public enum ApplicationStatus
{
    Draft, InReview, Approved, NotApproved, Withdrawn, NewMessage
}

public static class ApplicationStatusDisplay
{
    public static Dictionary<ApplicationStatus, string> Dictionary { get; } = new()
    {
        {ApplicationStatus.NewMessage, "New message" },
        { ApplicationStatus.Draft, "Draft" },
        { ApplicationStatus.InReview, "In review" },
        { ApplicationStatus.Approved, "Approved" },
        { ApplicationStatus.NotApproved, "Not approved" },
        { ApplicationStatus.Withdrawn, "Withdrawn" },
    };
}
