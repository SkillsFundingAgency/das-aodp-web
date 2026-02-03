using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
namespace SFA.DAS.AODP.Application.Queries.Review;

public class GetApplicationForReviewByIdQueryHandler : IRequestHandler<GetApplicationForReviewByIdQuery, BaseMediatrResponse<GetApplicationForReviewByIdQueryResponse>>
{
    private readonly IApiClient _apiCLient;
    public GetApplicationForReviewByIdQueryHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }

    public async Task<BaseMediatrResponse<GetApplicationForReviewByIdQueryResponse>> Handle(GetApplicationForReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationForReviewByIdQueryResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiCLient.Get<GetApplicationForReviewByIdQueryResponse>(new GetApplicationReviewByIdApiRequest(request.ApplicationReviewId));
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