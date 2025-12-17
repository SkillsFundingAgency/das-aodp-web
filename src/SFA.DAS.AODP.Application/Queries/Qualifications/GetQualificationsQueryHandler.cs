using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetQualificationsQueryHandler : IRequestHandler<GetQualificationsQuery, BaseMediatrResponse<GetQualificationsQueryResponse>>
{
    private readonly IApiClient _apiClient;

    public GetQualificationsQueryHandler(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<BaseMediatrResponse<GetQualificationsQueryResponse>> Handle(GetQualificationsQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetQualificationsQueryResponse>();
        response.Success = false;

        try
        {
            //var apiRequest = new GetQualificationsApiRequest()
            //{
            //    Skip = request.Skip,
            //    Take = request.Take,
            //    Name = request.Name,
            //    Organisation = request.Organisation,
            //    QAN = request.QAN,
            //    Status = request.Status
            //};

            //var result = await _apiClient.Get<GetQualificationsQueryResponse>(apiRequest);

            // Build dummy response instead of calling the API.
            var dummyResponse = new GetQualificationsQueryResponse
            {
                TotalRecords = 5,
                Skip = request.Skip,
                Take = request.Take,
                Data = new List<QualificationSearchResult>
                {
                    new QualificationSearchResult
                    {
                        Reference = "QAN-0001",
                        Title = "Level 1 - Introductory Qualification for Plumbing",
                        AwardingOrganisation = "Awarding Org A",
                        Status = "Active",
                        AgeGroup = "16-18"
                    },
                    new QualificationSearchResult
                    {
                        Reference = "QAN-0002",
                        Title = "Level 2 - Intermediate Qualification for Plumbing",
                        AwardingOrganisation = "Awarding Org B",
                        Status = "Active",
                        AgeGroup = "19-24"
                    },
                    new QualificationSearchResult
                    {
                        Reference = "QAN-0003",
                        Title = "Level 3 - Advanced Qualification for Plumbing",
                        AwardingOrganisation = "Awarding Org C",
                        Status = "Retired",
                        AgeGroup = "25+"
                    },
                    new QualificationSearchResult
                    {
                        Reference = "QAN-0004",
                        Title = "Specialist Certificate for Plumbing",
                        AwardingOrganisation = "Awarding Org D",
                        Status = "Active",
                        AgeGroup = "19-24"
                    },
                    new QualificationSearchResult
                    {
                        Reference = "QAN-0005",
                        Title = "Foundation Diploma for Plumbing",
                        AwardingOrganisation = "Awarding Org E",
                        Status = "Inactive",
                        AgeGroup = "16-18"
                    }
                }
            };

            await Task.CompletedTask;

            response.Value = dummyResponse;
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
