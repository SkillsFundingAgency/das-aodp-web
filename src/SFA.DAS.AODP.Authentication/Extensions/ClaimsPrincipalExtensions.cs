namespace SFA.DAS.AODP.Web.DfeSignIn.Extensions
{
    using System.Security.Claims;

    public static class ClaimsPrincipalExtensions
    {
        public static string GetClaimValue(this ClaimsPrincipal claimsPrincipal, string claimName)
        {
            var claim = claimsPrincipal.FindFirst(claimName);
            return claim != null ? claim.Value : string.Empty;
        }
    }
}
