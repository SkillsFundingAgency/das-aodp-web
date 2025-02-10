using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace SFA.DAS.AODP.Authentication.Services
{
    public interface IDfESignInService
    {
        Task PopulateAccountClaims(TokenValidatedContext ctx);
    }
}