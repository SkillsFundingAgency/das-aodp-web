using Microsoft.Extensions.Logging;

namespace SFA.DAS.AODP.Infrastructure.ApiClients
{
    public class AodpApiClient : IAodpApiClient
    {
        private readonly IApiClientHelper _clientHelper;
        private readonly ILogger<AodpApiClient> _logger;

        public AodpApiClient(IAodpApiClientFactory clientFactory, IApiClientHelper apiClientHelper, ILogger<AodpApiClient> logger)
        {
            _clientHelper = apiClientHelper;
            _clientHelper.SetApiClient(clientFactory.CreateHttpClient());

            _logger = logger;
        }

    }
}