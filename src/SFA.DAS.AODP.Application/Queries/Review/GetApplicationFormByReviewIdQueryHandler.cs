using MediatR;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Domain.Application.Review;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Models.Forms;

namespace SFA.DAS.AODP.Application.Queries.Review;

public partial class GetApplicationFormByReviewIdQueryHandler : IRequestHandler<GetApplicationFormByReviewIdQuery, BaseMediatrResponse<GetApplicationFormByReviewIdQueryResponse>>
{
    private readonly IApiClient _apiClient;
    public readonly IFileService _fileService;
    public GetApplicationFormByReviewIdQueryHandler(IApiClient apiClient, IFileService fileService)
    {
        _apiClient = apiClient;
        _fileService = fileService;
    }

    public async Task<BaseMediatrResponse<GetApplicationFormByReviewIdQueryResponse>> Handle(
    GetApplicationFormByReviewIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationFormByReviewIdQueryResponse>
        {
            Success = false
        };

        try
        {
            var questionAnswersResponse = await _apiClient.Get<GetApplicationFormAnswersByReviewIdApiResponse>(
                new GetApplicationFormAnswersByReviewIdApiRequest(request.ApplicationReviewId)
            );

            response.Value = questionAnswersResponse;


            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }

    private async Task<List<string>> GetFileNamesForApplication(Guid applicationId)
    {
        return _fileService.ListBlobs(applicationId.ToString())
            .Select(blob => blob.FileName)
            .ToList();
    }
}
