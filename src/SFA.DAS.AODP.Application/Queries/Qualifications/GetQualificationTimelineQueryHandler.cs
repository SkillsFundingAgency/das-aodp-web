using MediatR;
using SFA.DAS.AODP.Application.Services;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using SFA.DAS.AODP.Models.Qualifications;


namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetQualificationTimelineQueryHandler(IApiClient apiClient, IQualificationTimelineHistoryBuilder qualificationFieldChangeHistoryBuilder)
    : IRequestHandler<GetQualificationTimelineQuery, BaseMediatrResponse<QualificationDiscussionHistoriesResponse>>
{
    private const string DateTimeFormat = "MM/dd/yy HH:mm";
    private readonly IApiClient _apiClient = apiClient;
    private readonly IQualificationTimelineHistoryBuilder _qualificationFieldChangeHistoryBuilder = qualificationFieldChangeHistoryBuilder;

    public async Task<BaseMediatrResponse<QualificationDiscussionHistoriesResponse>> Handle(
        GetQualificationTimelineQuery request,
        CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<QualificationDiscussionHistoriesResponse>();

        try
        {
            var discussionHistoryResult = await _apiClient.Get<QualificationDiscussionHistoriesResponse>(
                new GetDiscussionHistoryForQualificationApiRequest(request.QualificationReference));

            if (discussionHistoryResult == null)
            {
                response.Success = false;
                response.ErrorMessage = $"Failed to get qualification discussion history for qualification ref: {request.QualificationReference}";
                return response;
            }

            var qualificationWithVersionsResult = await _apiClient.Get<GetQualificationDetailsQueryResponse>(
                new GetQualificationDetailWithVersionsApiRequest(request.QualificationReference));

            if (qualificationWithVersionsResult == null)
            {
                response.Success = false;
                response.ErrorMessage = $"Failed to get qualification versions for qualification ref: {request.QualificationReference}";
                return response;
            }

            discussionHistoryResult.QualificationDiscussionHistories ??= [];

            var versions = qualificationWithVersionsResult.Qual?.Versions?.ToList() ?? [];

            var generatedEntries = _qualificationFieldChangeHistoryBuilder.BuildTimelineEntries(versions);
            discussionHistoryResult.QualificationDiscussionHistories.AddRange(generatedEntries);

            response.Value = new QualificationDiscussionHistoriesResponse
            {
                QualificationDiscussionHistories = discussionHistoryResult.QualificationDiscussionHistories
                    .OrderByDescending(x => x.Timestamp)
                    .ToList()
            };
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