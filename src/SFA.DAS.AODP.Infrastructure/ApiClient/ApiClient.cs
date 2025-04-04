﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Models;
using SFA.DAS.AODP.Models.Settings;
using System.Net;
using System.Text;

namespace SFA.DAS.AODP.Infrastructure.ApiClient
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly AodpOuterApiSettings _config;

        public ApiClient(HttpClient httpClient, IOptions<AodpOuterApiSettings> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _httpClient.BaseAddress = new Uri(config.Value.BaseUrl);
        }

        public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
        {

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);
            AddAuthenticationHeader(requestMessage);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);
            }

            response.EnsureSuccessStatusCode();

            return default;
        }

        public async Task<TResponse> Put<TResponse>(IPutApiRequest request)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, request.PutUrl)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(request.Data), Encoding.UTF8, "application/json"),
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
            };
            AddAuthenticationHeader(requestMessage);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);


            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<TResponse>(json);

            }

            response.EnsureSuccessStatusCode();
            return default;
        }

        public async Task Put(IPutApiRequest request)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, request.PutUrl)
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(request.Data), Encoding.UTF8, "application/json"),
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower
            };
            AddAuthenticationHeader(requestMessage);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }

        public async Task<ApiResponse<TResponse>> PutWithResponseCode<TResponse>(IPutApiRequest request)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(request.Data), Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, request.PutUrl)
            {
                Content = stringContent,
            };
            AddAuthenticationHeader(requestMessage);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var QueryResponse = new ApiResponse<TResponse>(JsonConvert.DeserializeObject<TResponse>(responseContent), response.StatusCode, null);

            return QueryResponse;
        }

        public async Task<TResponse?> PostWithResponseCode<TResponse>(IPostApiRequest request)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(request.Data), Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, request.PostUrl)
            {
                Content = stringContent,
            };
            AddAuthenticationHeader(requestMessage);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<TResponse>(responseContent) ?? default;
        }

        public async Task PostWithResponseCode(IPostApiRequest request)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(request.Data), Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, request.PostUrl)
            {
                Content = stringContent,
            };
            AddAuthenticationHeader(requestMessage);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        public async Task Delete(IDeleteApiRequest request)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, request.DeleteUrl);
            AddAuthenticationHeader(requestMessage);

            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }

        private void AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _config.Key);
            httpRequestMessage.Headers.Add("X-Version", "1");
        }
    }
}
