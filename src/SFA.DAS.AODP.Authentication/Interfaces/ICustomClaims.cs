using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;

namespace SFA.DAS.AODP.Authentication.Interfaces
{
    public interface ICustomClaims
    {
        IEnumerable<Claim> GetClaims(TokenValidatedContext tokenValidatedContext);
    }
}
