using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using SFA.DAS.AODP.Common.Exceptions;
using System.Net;

namespace SFA.DAS.AODP.Infrastructure.ApiClients
{

    //TODO: review and remove unnecessary code
    public class ApiClientHelper : IApiClientHelper
    {
        private HttpClient _httpClient;
        private AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public void SetApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)));
        }

        //Example from: https://github.com/SkillsFundingAgency/das-assessor-service/blob/9f68662fea72909d81008c10ad89601bb21218cf/src/SFA.DAS.AssessorService.Api.Common/ApiClientBase.cs 
        
        //public async Task<T> RequestAndDeserialiseAsync<T>(HttpRequestMessage request, string message = null, bool mapNotFoundToNull = false)
        //{
        //    HttpRequestMessage clonedRequest = null;

        //    var result = await _retryPolicy.ExecuteAsync(async () =>
        //    {
        //        clonedRequest = new HttpRequestMessage(request.Method, request.RequestUri);
        //        return await _httpClient.SendAsync(clonedRequest);
        //    });

        //    if (result.StatusCode == HttpStatusCode.OK)
        //    {
        //        // NOTE: Struct values are valid JSON. For example: 'True'
        //        var json = await result.Content.ReadAsStringAsync();
        //        return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(json, _jsonSettings));
        //    }
        //    else if (result.StatusCode == HttpStatusCode.NoContent)
        //    {
        //        return default;
        //    }
        //    else if (result.StatusCode == HttpStatusCode.NotFound)
        //    {
        //        if (!mapNotFoundToNull)
        //        {
        //            if (message == null)
        //            {
        //                if (!request.RequestUri.IsAbsoluteUri)
        //                    message = "Could not find " + request.RequestUri;
        //                else
        //                    message = "Could not find " + request.RequestUri.PathAndQuery;
        //            }

        //            RaiseResponseError(message, clonedRequest, result);
        //        }
        //    }
        //    else
        //        RaiseResponseError(clonedRequest, result);

        //    return default;
        //}


        private static void RaiseResponseError(string message, HttpRequestMessage failedRequest, HttpResponseMessage failedResponse)
        {
            if (failedResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new EntityNotFoundException(message, CreateRequestException(failedRequest, failedResponse));
            }

            throw CreateRequestException(failedRequest, failedResponse);
        }

        private static void RaiseResponseError(HttpRequestMessage failedRequest, HttpResponseMessage failedResponse)
        {
            throw CreateRequestException(failedRequest, failedResponse);
        }

        private static HttpRequestException CreateRequestException(HttpRequestMessage failedRequest, HttpResponseMessage failedResponse)
        {
            return new HttpRequestException(
                string.Format($"The Client request for {{0}} {{1}} failed. Response Status: {{2}}, Response Body: {{3}}",
                    failedRequest.Method.ToString().ToUpperInvariant(),
                    failedRequest.RequestUri,
                    (int)failedResponse.StatusCode,
                    failedResponse.Content.ReadAsStringAsync().Result));
        }
    }
}