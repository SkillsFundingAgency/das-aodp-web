using SFA.DAS.AODP.Domain.Interfaces;

public class GetUsersApiRequest : IGetApiRequest
{
    public string GetUrl => $"api/users/reviewers";
}