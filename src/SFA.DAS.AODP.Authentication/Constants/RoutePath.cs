namespace SFA.DAS.AODP.Authentication.Constants
{
    public static class RoutePath
    {
        /// <summary>
        /// Route path the authentication provider posts back when authenticating.
        /// </summary>
        public const string OidcSignIn = "/sign-in";
        /// <summary>
        /// Route path the authentication provider posts back after signing out.
        /// </summary>
        public const string OidcSignOut = "/signout-callback-oidc";
    }
}
