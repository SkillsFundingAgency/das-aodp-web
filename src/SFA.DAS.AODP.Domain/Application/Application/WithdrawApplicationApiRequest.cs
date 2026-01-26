using SFA.DAS.AODP.Domain.Interfaces;

public class WithdrawApplicationApiRequest : IPostApiRequest
{
    public Guid ApplicationId { get; set; }

    public string PostUrl => $"api/applications/{ApplicationId}/withdraw";

    public required object Data { get; set; }

}
