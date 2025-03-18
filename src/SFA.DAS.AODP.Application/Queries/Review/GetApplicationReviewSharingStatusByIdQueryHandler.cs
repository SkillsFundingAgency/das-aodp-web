using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationReviewSharingStatusByIdQueryHandler : IRequestHandler<GetApplicationReviewSharingStatusByIdQuery, BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>>
{
    private readonly IApiClient _apiCLient;
    public GetApplicationReviewSharingStatusByIdQueryHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }

    public async Task<BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>> Handle(GetApplicationReviewSharingStatusByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationReviewSharingStatusByIdQueryResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiCLient.Get<GetApplicationReviewSharingStatusByIdQueryResponse>(new GetApplicationReviewSharingStatusByIdApiRequest(request.ApplicationReviewId));
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