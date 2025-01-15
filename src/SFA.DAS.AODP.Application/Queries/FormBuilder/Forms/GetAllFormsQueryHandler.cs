using MediatR;
using SFA.DAS.AODP.Domain.Forms.GetAllForms;
using SFA.DAS.FAA.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetAllFormsQueryHandler : IRequestHandler<GetAllFormsQuery, GetAllFormsQueryResponse>
{
    private readonly IApiClient _apiClient;

    public GetAllFormsQueryHandler(IApiClient client)
    {
        _apiClient = client;
    }

    public async Task<GetAllFormsQueryResponse> Handle(GetAllFormsQuery request, CancellationToken cancellationToken)
    {
        var queryResponse = new GetAllFormsQueryResponse
        {
            Success = false
        };
        try
        {
            var apiResponse = await _apiClient.Get<GetAllFormsResponse>(new GetAllFormsRequest());

            queryResponse.Data = apiResponse.Forms;
            queryResponse.Success = true;
        }
        catch (Exception ex)
        {
            queryResponse.ErrorMessage = ex.Message;
        }

        return queryResponse;
    }
}