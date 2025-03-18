using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class GetQualificationVersionsForQualificationByReferenceQueryHandler : IRequestHandler<GetQualificationVersionsForQualificationByReferenceQuery, BaseMediatrResponse<GetQualificationVersionsForQualificationByReferenceQueryResponse>>
{
    private readonly IApiClient _apiCLient;
    public GetQualificationVersionsForQualificationByReferenceQueryHandler(IApiClient apiCLient)
    {
        _apiCLient = apiCLient;
    }

    public async Task<BaseMediatrResponse<GetQualificationVersionsForQualificationByReferenceQueryResponse>> Handle(GetQualificationVersionsForQualificationByReferenceQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetQualificationVersionsForQualificationByReferenceQueryResponse>
        {
            Success = false
        };

        try
        {
            var result = await _apiCLient.Get<GetQualificationVersionsForQualificationByReferenceQueryResponse>(new GetQualificationVersionsForQualificationByReferenceApiRequest(request.QualificationReference));

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