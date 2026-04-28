using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
namespace SFA.DAS.AODP.Web.UnitTests.Extensions.Startup;

public class SecurityHeadersMiddlewareTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityHeadersMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Security_headers_are_present_and_correct()
    {
        // Act
        var response = await _client.GetAsync("/");

        response.EnsureSuccessStatusCode();

        // Assert — Individual headers
        Assert.Equal(
            "max-age=31536000; includeSubDomains",
            response.Headers.GetValues("Strict-Transport-Security").Single());

        Assert.Equal(
            "nosniff",
            response.Headers.GetValues("X-Content-Type-Options").Single());

        Assert.Equal(
            "SAMEORIGIN",
            response.Headers.GetValues("X-Frame-Options").Single());

        Assert.Equal(
            "none",
            response.Headers.GetValues("X-Permitted-Cross-Domain-Policies").Single());
    }

    [Fact]
    public async Task ContentSecurityPolicy_contains_all_required_sources()
    {
        var response = await _client.GetAsync("/");

        response.EnsureSuccessStatusCode();

        var csp = response.Headers
            .GetValues("Content-Security-Policy")
            .Single();

        // Explicitly allowed BEFORE 
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("img-src 'self' *.azureedge.net *.google-analytics.com", csp);
        Assert.Contains("script-src 'self' 'unsafe-inline'", csp);
        Assert.Contains("*.googletagmanager.com", csp);
        Assert.Contains("*.google-analytics.com", csp);
        Assert.Contains("*.googleapis.com", csp);
        Assert.Contains("style-src 'self' 'unsafe-inline' *.azureedge.net", csp);
        Assert.Contains("style-src-elem 'self' *.azureedge.net", csp);
        Assert.Contains("font-src 'self' *.azureedge.net data:", csp);
        Assert.Contains("connect-src 'self'", csp);
    }

}
