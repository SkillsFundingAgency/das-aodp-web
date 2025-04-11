using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetQfauFeedbackForApplicationReviewConfirmationQueryHandler : IRequestHandler<GetQfauFeedbackForApplicationReviewConfirmationQuery, BaseMediatrResponse<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>>
{
    private readonly IApiClient _apiCLient;
    public GetQfauFeedbackForApplicationReviewConfirmationQueryHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }

    public async Task<BaseMediatrResponse<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>> Handle(GetQfauFeedbackForApplicationReviewConfirmationQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiCLient.Get<GetQfauFeedbackForApplicationReviewConfirmationQueryResponse>(new GetQfauFeedbackForApplicationReviewConfirmationApiRequest(request.ApplicationReviewId));

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