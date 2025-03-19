using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetFeedbackForQualificationFundingByIdQueryHandler : IRequestHandler<GetFeedbackForQualificationFundingByIdQuery, BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>>
{
    private readonly IApiClient _apiCLient;
    public GetFeedbackForQualificationFundingByIdQueryHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }

    public async Task<BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>> Handle(GetFeedbackForQualificationFundingByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiCLient.Get< BaseMediatrResponse<GetFeedbackForQualificationFundingByIdQueryResponse>>(new GetFeedbackForQualificationFundingByIdApiRequest(request.QualificationVersionId));

            response.Value = result.Value;

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