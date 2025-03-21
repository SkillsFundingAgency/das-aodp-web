namespace SFA.DAS.AODP.Models.Qualifications;

public enum ActionType
{
    NoActionRequired, 
    ActionRequired, 
    Ignore
}

public static class ActionTypeDisplay
{
    public static Dictionary<ActionType, string> Dictionary { get; } = new()
    {
        { ActionType.NoActionRequired, "00000000-0000-0000-0000-000000000001" },
        { ActionType.ActionRequired, "00000000-0000-0000-0000-000000000002" },
        { ActionType.Ignore, "00000000-0000-0000-0000-000000000003" },
    };
}
