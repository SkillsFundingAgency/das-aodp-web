using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace SFA.DAS.AODP.Web.DfeSignIn.Services
{
    public interface IDfESignInService
    {
        Task PopulateAccountClaims(TokenValidatedContext ctx);
    }
}