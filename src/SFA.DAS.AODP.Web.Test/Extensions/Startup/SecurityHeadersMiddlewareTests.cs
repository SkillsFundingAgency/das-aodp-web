using Microsoft.AspNetCore.Mvc.Testing;

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

        // Assert

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
    public async Task ContentSecurityPolicy_contains_expected_directives_and_is_secure()
    {
        var response = await _client.GetAsync("/");

        response.EnsureSuccessStatusCode();

        var csp = response.Headers
            .GetValues("Content-Security-Policy")
            .Single();

        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("img-src", csp);
        Assert.Contains("script-src", csp);
        Assert.Contains("style-src", csp);
        Assert.Contains("font-src", csp);
        Assert.Contains("connect-src", csp);

        Assert.Contains("*.azureedge.net", csp);
        Assert.Contains("*.googletagmanager.com", csp);
        Assert.Contains("*.google-analytics.com", csp);
        Assert.Contains("*.googleapis.com", csp);

        Assert.DoesNotContain("'unsafe-inline'", csp);
    }
}