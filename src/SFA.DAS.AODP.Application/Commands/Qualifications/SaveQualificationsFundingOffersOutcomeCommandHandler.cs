using MediatR;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Domain.Interfaces;

public class SaveQualificationsFundingOffersOutcomeCommandHandler : IRequestHandler<SaveQualificationsFundingOffersOutcomeCommand, BaseMediatrResponse<EmptyResponse>>
{
    private readonly IApiClient _apiClient;


    public SaveQualificationsFundingOffersOutcomeCommandHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<EmptyResponse>> Handle(SaveQualificationsFundingOffersOutcomeCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<EmptyResponse>()
        {
            Success = false
        };

        try
        {
            var apiRequest = new SaveQualificationsFundingOffersOutcomeApiRequest()
            {
                QualificationVersionId = request.QualificationVersionId,
                Data = request
            };
            await _apiClient.Put(apiRequest);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            response.Success = false;
        }

        return response;
    }

}