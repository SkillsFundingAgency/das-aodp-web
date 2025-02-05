using Newtonsoft.Json;
using SFA.DAS.AODP.Authentication.DfeSignInApi.JWTHelpers;
using System.Net.Http.Headers;

namespace SFA.DAS.AODP.Authentication.DfeSignInApi.Client
{
    public class DFESignInAPIClient : IDFESignInAPIClient
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenBuilder _tokenBuilder;


        public DFESignInAPIClient(HttpClient httpClient, ITokenBuilder tokenBuilder)
        {
            _httpClient = httpClient;
            _tokenBuilder = tokenBuilder;
        }

        public async Task<T> Get<T>(string endpoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenBuilder.CreateToken());
            var getResponse = await _httpClient.SendAsync(request);
            return getResponse.IsSuccessStatusCode ? JsonConvert.DeserializeObject<T>(await getResponse.Content.ReadAsStringAsync()) : default;
        }
    }
}
