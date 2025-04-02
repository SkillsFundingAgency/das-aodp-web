using System.Security.Claims;

public static class UserClaimsHelper
{
    public static List<Claim> GetUserClaims(HttpContext httpContext, string claimType = "rolecode")
    {
        if (httpContext == null || httpContext.User == null)
            return new List<Claim>();

        return httpContext.User.Claims
            .Where(c => c.Type == claimType)
            .ToList();
    }
}
