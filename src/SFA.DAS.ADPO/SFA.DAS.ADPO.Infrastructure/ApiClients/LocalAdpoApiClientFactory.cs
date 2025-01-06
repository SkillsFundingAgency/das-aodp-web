using SFA.DAS.Http;

public class LocalAdpoApiClientFactory : IAdpoApiClientFactory
{
    private readonly AdpoApiClientConfiguration _adpoApiClientConfiguration;

    public LocalAdpoApiClientFactory(AdpoApiClientConfiguration adpoApiClientConfiguration)
    {
        _adpoApiClientConfiguration = adpoApiClientConfiguration;
    }

    public HttpClient CreateHttpClient()
    {
        var builder = new HttpClientBuilder();
        var httpClient = builder.WithDefaultHeaders().Build();
        httpClient.BaseAddress = new System.Uri(_adpoApiClientConfiguration.ApiBaseUrl);
        return httpClient;
    }
}
