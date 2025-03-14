using MediatR;
using SFA.DAS.AODP.Domain.Application.Review;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Review;

public partial class GetApplicationReadOnlyDetailsByIdQueryHandler : IRequestHandler<GetApplicationReadOnlyDetailsByIdQuery, BaseMediatrResponse<GetApplicationReadOnlyDetailsByIdQueryResponse>>
{
    private readonly IApiClient _apiCLient;
    public GetApplicationReadOnlyDetailsByIdQueryHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }

    public async Task<BaseMediatrResponse<GetApplicationReadOnlyDetailsByIdQueryResponse>> Handle(GetApplicationReadOnlyDetailsByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetApplicationReadOnlyDetailsByIdQueryResponse>
        {
            Success = false
        };

        try
        {
            var application = await _apiCLient.Get<GetApplicationForReviewByIdQueryResponse>(new GetApplicationReviewByIdApiRequest(request.ApplicationReviewId));

            var result = await _apiCLient.Get<GetApplicationReadOnlyDetailsByIdQueryResponse>(new GetApplicationReadOnlyDetailsByIdApiRequest(application.Id));
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