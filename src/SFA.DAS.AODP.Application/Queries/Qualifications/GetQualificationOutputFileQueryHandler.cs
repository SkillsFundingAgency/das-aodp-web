using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationOutputFileQueryHandler : IRequestHandler<GetQualificationOutputFileQuery, BaseMediatrResponse<GetQualificationOutputFileResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetQualificationOutputFileQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetQualificationOutputFileResponse>> Handle(GetQualificationOutputFileQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _apiClient.PostWithResponseCode<BaseMediatrResponse<GetQualificationOutputFileResponse>>(
                    new GetQualificationOutputFileApiRequest { Data = request});

                if (result == null)
                {
                    return new BaseMediatrResponse<GetQualificationOutputFileResponse>
                    {
                        Success = false,
                        ErrorMessage = "No response received from inner API",
                        ErrorCode = ErrorCodes.UnexpectedError
                    };
                }
                
                if (!result.Success || result.Value is null ||
                    result.Value.FileContent is null || result.Value.FileContent.Length == 0)
                {
                    return new BaseMediatrResponse<GetQualificationOutputFileResponse>
                    {
                        Success = false,
                        ErrorMessage = result?.ErrorMessage ?? "Unexpected error",
                        ErrorCode = result?.ErrorCode ?? ErrorCodes.UnexpectedError
                    };
                }

                return new BaseMediatrResponse<GetQualificationOutputFileResponse>
                {
                    Success = true,
                    Value = result.Value,
                };
            }
            catch (Exception ex)
            {
                return new BaseMediatrResponse<GetQualificationOutputFileResponse>
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = ErrorCodes.UnexpectedError
                };
            }
        }
    }
}
