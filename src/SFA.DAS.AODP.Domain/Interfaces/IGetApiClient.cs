using SFA.DAS.AODP.Domain.Models;
using System.Net;

namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IGetApiClient<T>
{
    Task<TResponse> Get<TResponse>(IGetApiRequest request);
    Task<HttpStatusCode> GetResponseCode(IGetApiRequest request);
    Task<ApiResponse<TResponse>> GetWithResponseCode<TResponse>(IGetApiRequest request);
}
