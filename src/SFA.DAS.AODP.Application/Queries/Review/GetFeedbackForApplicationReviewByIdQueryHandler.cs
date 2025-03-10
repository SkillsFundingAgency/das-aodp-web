using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetFeedbackForApplicationReviewByIdQueryHandler : IRequestHandler<GetFeedbackForApplicationReviewByIdQuery, BaseMediatrResponse<GetFeedbackForApplicationReviewByIdQueryResponse>>
{
    private readonly IApiClient _apiCLient;
    public GetFeedbackForApplicationReviewByIdQueryHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }

    public async Task<BaseMediatrResponse<GetFeedbackForApplicationReviewByIdQueryResponse>> Handle(GetFeedbackForApplicationReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetFeedbackForApplicationReviewByIdQueryResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiCLient.Get<GetFeedbackForApplicationReviewByIdQueryResponse>(new GetFeedbackForApplicationReviewByIdApiRequest(request.ApplicationReviewId, request.UserType));

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