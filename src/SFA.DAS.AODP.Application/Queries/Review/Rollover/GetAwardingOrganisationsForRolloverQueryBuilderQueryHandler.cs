using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetAwardingOrganisationsForRolloverQueryBuilderQueryHandler(IApiClient apiClient)
    : IRequestHandler<GetAwardingOrganisationsForRolloverQueryBuilderQuery, BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>>
{
    public async Task<BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>> Handle(
        GetAwardingOrganisationsForRolloverQueryBuilderQuery request,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.PostWithResponseCode<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>(
            new GetAwardingOrganisationsForRolloverQueryBuilderApiRequest(request.Filters));

        return new BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>
        {
            Success = true,
            Value = result ?? new GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse()
        };
    }
}
