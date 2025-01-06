using Microsoft.Extensions.Logging;

namespace SFA.DAS.ADPO.Infrastructure.ApiClients
{
    public class AdpoApiClient : IAdpoApiClient
    {
        private readonly IApiClientHelper _clientHelper;
        private readonly ILogger<AdpoApiClient> _logger;

        public AdpoApiClient(IAdpoApiClientFactory clientFactory, IApiClientHelper apiClientHelper, ILogger<AdpoApiClient> logger)
        {
            _clientHelper = apiClientHelper;
            _clientHelper.SetApiClient(clientFactory.CreateHttpClient());

            _logger = logger;
        }

    }
}