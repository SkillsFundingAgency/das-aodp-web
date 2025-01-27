using SFA.DAS.AODP.Domain.Models;

namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IApiClient
{
    Task<TResponse> Get<TResponse>(IGetApiRequest request);
    Task<TResponse> Put<TResponse>(IPutApiRequest request);
    Task<TResponse?> PostWithResponseCode<TResponse>(IPostApiRequest request);
    Task<ApiResponse<TResponse>> PutWithResponseCode<TResponse>(IPutApiRequest request);
    Task PostWithResponseCode(IPostApiRequest request);
    Task Delete(IDeleteApiRequest request);
    Task Put(IPutApiRequest request);
}