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

                // Enforce HTTPS (explicit requirement)
                headers["Strict-Transport-Security"] =
                    "max-age=31536000; includeSubDomains";

                // Prevent MIME sniffing (explicit requirement)
                headers["X-Content-Type-Options"] = "nosniff";

                // Clickjacking protection (required and relied upon)
                headers["X-Frame-Options"] = "SAMEORIGIN";

                // Content Security Policy — RESTORED to previously explicit values
                headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "img-src 'self' *.azureedge.net *.google-analytics.com; " +
                    "script-src 'self' 'unsafe-inline' *.azureedge.net *.googletagmanager.com *.google-analytics.com *.googleapis.com; " +
                    "style-src 'self' 'unsafe-inline' *.azureedge.net; " +
                    "style-src-elem 'self' *.azureedge.net; " +
                    "font-src 'self' *.azureedge.net data:; " +
                    "connect-src 'self' http://localhost:* https://localhost:* ws://localhost:* wss://localhost:* https://*.google-analytics.com;";

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