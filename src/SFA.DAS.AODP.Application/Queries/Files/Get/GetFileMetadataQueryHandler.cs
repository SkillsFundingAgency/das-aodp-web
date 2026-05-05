using MediatR;
using SFA.DAS.AODP.Domain.Files;
using SFA.DAS.AODP.Domain.Interfaces;
namespace SFA.DAS.AODP.Application.Queries.Files.Get
{

    public class GetFileMetadataQueryHandler : IRequestHandler<GetFileMetadataQuery, BaseMediatrResponse<GetFileMetadataQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetFileMetadataQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetFileMetadataQueryResponse>> Handle(
             GetFileMetadataQuery request,
             CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetFileMetadataQueryResponse>();

            try
            {
                var result = await _apiClient
                    .PostWithResponseCode<GetFileMetadataQueryResponse>(
                        new GetFileMetadataApiRequest { Data = request });

                if (result == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "File metadata service returned no data.";
                    return response;
                }

                response.Value = result;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
    }
}
