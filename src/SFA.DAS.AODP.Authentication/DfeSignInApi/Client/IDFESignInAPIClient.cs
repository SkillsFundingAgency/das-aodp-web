namespace SFA.DAS.AODP.Authentication.DfeSignInApi.Client
{
    public interface IDFESignInAPIClient
    {
        Task<T> Get<T>(string endpoint);
    }
}
