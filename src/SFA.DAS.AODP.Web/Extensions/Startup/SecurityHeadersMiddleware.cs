namespace SFA.DAS.AODP.Web.Extensions.Startup
{
    public sealed class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                // Prevent MIME sniffing (explicit requirement)
                headers.XContentTypeOptions = "nosniff";

                // Clickjacking protection (required and relied upon)
                headers.XFrameOptions = "SAMEORIGIN";

                // Content Security Policy — RESTORED to previously explicit values
                headers.ContentSecurityPolicy =
                    "default-src 'self'; " +
                    "img-src 'self' *.azureedge.net *.google-analytics.com; " +
                    "script-src 'self' *.azureedge.net *.googletagmanager.com *.google-analytics.com *.googleapis.com; " +
                    "style-src 'self' *.azureedge.net; " +
                    "style-src-elem 'self' *.azureedge.net; " +
                    "font-src 'self' *.azureedge.net data:; " +
                    "connect-src 'self' https://localhost:* ws://localhost:* wss://localhost:* https://*.google-analytics.com;";

                // Browser feature restrictions (already present before)
                headers["Permissions-Policy"] =
                    "geolocation=(), microphone=(), camera=()";

                // Legacy cross-domain protections (explicit requirement)
                headers["X-Permitted-Cross-Domain-Policies"] = "none";

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}