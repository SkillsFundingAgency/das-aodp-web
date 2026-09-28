using SFA.DAS.AODP.Models.Qualifications;

namespace SFA.DAS.AODP.Application.Queries.Review.Rollover;

public class GetAwardingOrganisationsForRolloverQueryBuilderQueryHandler(IApiClient apiClient)
    : IRequestHandler<GetAwardingOrganisationsForRolloverQueryBuilderQuery, BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>>
{
    public async Task<BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>> Handle(
        GetAwardingOrganisationsForRolloverQueryBuilderQuery request,
        CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>();

        try
        {
            var result = await apiClient.PostWithResponseCode<GetAwardingOrganisationsForRolloverQueryBuilderQueryResponse>(
                new GetAwardingOrganisationsForRolloverQueryBuilderApiRequest(request.Filters));

            if (result is null)
            {
                response.Value.AwardingOrganisations = Enumerable.Empty<AwardingOrganisation>();
                response.Success = true;

                return response;
            }

            response.Success = true;
            response.Value.AwardingOrganisations = result.AwardingOrganisations.Select(o => new AwardingOrganisation
            {
                Id = o.Id,
                Acronym = o.Acronym,
                Name_Dsi = o.Name_Dsi,
                NameGovUk = o.NameGovUk,
                NameLegal = o.NameLegal,
                NameOfqual = o.NameOfqual,
                RecognitionNumber = o.RecognitionNumber,
                Ukprn = o.Ukprn
            });
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}