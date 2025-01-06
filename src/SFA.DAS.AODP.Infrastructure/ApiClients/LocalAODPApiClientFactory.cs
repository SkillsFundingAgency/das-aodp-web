using SFA.DAS.Http;

public class LocalAodpApiClientFactory : IAodpApiClientFactory
{
    private readonly AodpApiClientConfiguration _aodpApiClientConfiguration;

    public LocalAodpApiClientFactory(AodpApiClientConfiguration aodpApiClientConfiguration)
    {
        _aodpApiClientConfiguration = aodpApiClientConfiguration;
    }

    public HttpClient CreateHttpClient()
    {
        var builder = new HttpClientBuilder();
        var httpClient = builder.WithDefaultHeaders().Build();
        httpClient.BaseAddress = new System.Uri(_aodpApiClientConfiguration.ApiBaseUrl);
        return httpClient;
    }
}
