namespace SFA.DAS.AODP.Web.DfeSignIn.DfeSignInApi.Client
{
    public interface IDFESignInAPIClient
    {
        Task<T> Get<T>(string endpoint);
    }
}
