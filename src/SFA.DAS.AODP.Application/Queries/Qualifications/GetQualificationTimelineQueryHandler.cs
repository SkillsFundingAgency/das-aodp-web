using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;
using SFA.DAS.AODP.Models.Qualifications;


namespace SFA.DAS.AODP.Application.Queries.Qualifications;

public class GetQualificationTimelineQueryHandler(IApiClient apiClient)
    : IRequestHandler<GetQualificationTimelineQuery, BaseMediatrResponse<QualificationDiscussionHistoriesResponse>>
{
    private const string DateTimeFormat = "MM/dd/yy HH:mm";
    private readonly IApiClient _apiClient = apiClient;

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

            AddFundingEligibilityEntries(discussionHistoryResult.QualificationDiscussionHistories, versions);
            AddFieldChangeEntries(discussionHistoryResult.QualificationDiscussionHistories, versions);

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

    #region Changed Fields History

    private static void AddFieldChangeEntries(
        List<QualificationDiscussionHistory> discussionHistoryList,
        List<GetQualificationDetailsQueryResponse> versions)
    {
        if (versions.Count == 0)
        {
            return;
        }

        var latestVersionNumber = versions.Max(i => i.Version) ?? 0;
        var currentVersion = versions.FirstOrDefault(v => v.Version == latestVersionNumber);

        if (latestVersionNumber <= 1 || currentVersion == null)
        {
            return;
        }

        for (int? i = latestVersionNumber; i > 1; i--)
        {
            if (i != latestVersionNumber)
            {
                currentVersion = versions.FirstOrDefault(v => v.Version == i);
            }

            var previousVersion = versions.FirstOrDefault(v => v.Version == i - 1);

            if (currentVersion == null || previousVersion == null)
            {
                continue;
            }

            var changeEntry = BuildChangeHistoryEntry(currentVersion, previousVersion);

            if (changeEntry != null)
            {
                discussionHistoryList.Add(changeEntry);
            }
        }
    }

    private static List<FieldChange> BuildKeyFieldChanges(
            GetQualificationDetailsQueryResponse latestVersion,
            GetQualificationDetailsQueryResponse previousVersion)
    {
        var changedFieldNames = latestVersion.VersionFieldChanges?.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

        var changes = new List<FieldChange>();

        foreach (var item in changedFieldNames)
        {
            switch (item)
            {
                case "EligibleForFunding":
                    changes.Add(new FieldChange(
                        "eligible for funding",
                        previousVersion.EligibleForFunding.ToString(),
                        latestVersion.EligibleForFunding.ToString()));
                    break;

                case "OrganisationName":
                    changes.Add(new FieldChange(
                        "organisation name",
                        previousVersion.Organisation.NameOfqual,
                        latestVersion.Organisation.NameOfqual));
                    break;

                case "Title":
                    changes.Add(new FieldChange(
                        "title",
                        previousVersion.Qual.QualificationName,
                        latestVersion.Qual.QualificationName));
                    break;

                case "Level":
                    changes.Add(new FieldChange(
                        "level",
                        previousVersion.Level.ToString(),
                        latestVersion.Level.ToString()));
                    break;

                case "Type":
                    changes.Add(new FieldChange(
                        "type",
                        previousVersion.Type,
                        latestVersion.Type));
                    break;

                case "TotalCredits":
                    changes.Add(new FieldChange(
                        "total credits",
                        previousVersion.TotalCredits.ToString(),
                        latestVersion.TotalCredits.ToString()));
                    break;

                case "Ssa":
                    changes.Add(new FieldChange(
                        "SSA",
                        previousVersion.Ssa.ToString(),
                        latestVersion.Ssa.ToString()));
                    break;

                case "GradingType":
                    changes.Add(new FieldChange(
                        "grading type",
                        previousVersion.GradingType?.ToString(),
                        latestVersion.GradingType?.ToString()));
                    break;

                case "OfferedInEngland":
                    changes.Add(new FieldChange(
                        "offered in england",
                        previousVersion.OfferedInEngland.ToString(),
                        latestVersion.OfferedInEngland.ToString()));
                    break;

                case "IntentionToSeekFundingInEngland":
                    changes.Add(new FieldChange(
                        "intention to seek funding in england",
                        previousVersion.IntentionToSeekFundingInEngland.ToString(),
                        latestVersion.IntentionToSeekFundingInEngland.ToString()));
                    break;

                case "PreSixteen":
                    changes.Add(new FieldChange(
                        "pre-sixteen",
                        previousVersion.PreSixteen.ToString(),
                        latestVersion.PreSixteen.ToString()));
                    break;

                case "SixteenToEighteen":
                    changes.Add(new FieldChange(
                        "sixteen to eighteen",
                        previousVersion.SixteenToEighteen.ToString(),
                        latestVersion.SixteenToEighteen.ToString()));
                    break;

                case "EighteenPlus":
                    changes.Add(new FieldChange(
                        "eighteen plus",
                        previousVersion.EighteenPlus.ToString(),
                        latestVersion.EighteenPlus.ToString()));
                    break;

                case "NineteenPlus":
                    changes.Add(new FieldChange(
                        "nineteen plus",
                        previousVersion.NineteenPlus.ToString(),
                        latestVersion.NineteenPlus.ToString()));
                    break;

                case "Glh":
                    changes.Add(new FieldChange(
                        "guided learning hours (GLH)",
                        previousVersion.Glh.ToString(),
                        latestVersion.Glh.ToString()));
                    break;

                case "MinimumGlh":
                    changes.Add(new FieldChange(
                        "minimum glh",
                        previousVersion.MinimumGlh.ToString(),
                        latestVersion.MinimumGlh.ToString()));
                    break;

                case "Tqt":
                    changes.Add(new FieldChange(
                        "total qualification time (TQT)",
                        previousVersion.Tqt.ToString(),
                        latestVersion.Tqt.ToString()));
                    break;

                case "OperationalEndDate":
                    changes.Add(new FieldChange(
                        "operational end date",
                        previousVersion.OperationalEndDate?.ToString(DateTimeFormat),
                        latestVersion.OperationalEndDate?.ToString(DateTimeFormat)));
                    break;

                case "LastUpdatedDate":
                    changes.Add(new FieldChange(
                        "last updated date",
                        previousVersion.LastUpdatedDate.ToString(DateTimeFormat),
                        latestVersion.LastUpdatedDate.ToString(DateTimeFormat)));
                    break;

                case "Version":
                    changes.Add(new FieldChange(
                        "version",
                        previousVersion.Version.ToString(),
                        latestVersion.Version.ToString()));
                    break;

                case "OfferedInternationally":
                    changes.Add(new FieldChange(
                        "offered internationally",
                        previousVersion.OfferedInternationally.ToString(),
                        latestVersion.OfferedInternationally.ToString()));
                    break;
            }
        }

        return changes;
    }

    private static QualificationDiscussionHistory? BuildChangeHistoryEntry(
        GetQualificationDetailsQueryResponse latestVersion,
        GetQualificationDetailsQueryResponse previousVersion)
    {
        var keyFieldChanges = BuildKeyFieldChanges(latestVersion, previousVersion);

        if (keyFieldChanges.Count == 0)
        {
            return null;
        }

        var notes = BuildChangeString(keyFieldChanges);

        return (new()
        {
            Notes = notes,
            Title = "Change",
            UserDisplayName = "OFQUAL Import",
            Timestamp = latestVersion.InsertedTimestamp,
            ActionType = new ActionType
            {
                Id = Guid.Empty,
                Description = "change"
            }
        });
    }

    private static string BuildChangeString(List<FieldChange> fieldChanges)
    {
        var items = fieldChanges
            .Select(x => $"{x.Name} changed from {x.Was?.ToLower()} to {x.Now?.ToLower()}")
            .ToList();

        return TimelineFormattingHelper.ToDisplayString(
            items,
            string.Empty,
            "The following fields changed:");
    }

    #endregion Changed Fields History

    #region Funding Eligibility History

    private static void AddFundingEligibilityEntries(
        List<QualificationDiscussionHistory> discussionHistoryList,
        List<GetQualificationDetailsQueryResponse> versions)
    {
        foreach (var version in versions)
        {
            var fundingEntry = BuildFundingEligibilityHistoryEntry(version);

            if (fundingEntry != null)
            {
                discussionHistoryList.Add(fundingEntry);
            }
        }
    }

    private static QualificationDiscussionHistory? BuildFundingEligibilityHistoryEntry(
        GetQualificationDetailsQueryResponse qualificationVersion)
    {
        if (string.IsNullOrWhiteSpace(qualificationVersion.FundingEligibilityFailedFields))
        {
            return null;
        }

        var fundingStatus = new EligibleForFundingStatus(
            qualificationVersion.EligibleForFunding,
            qualificationVersion.FundingEligibilityFailedFields
        );

        var fundingNotes = fundingStatus.ToDisplayString();

        if (string.IsNullOrWhiteSpace(fundingNotes))
        {
            return null;
        }

        return new()
        {
            Notes = fundingNotes,
            Title = "Change",
            UserDisplayName = "OFQUAL Import",
            Timestamp = qualificationVersion.InsertedTimestamp,
            ActionType = new ActionType
            { 
                Id = Guid.Empty,
                Description = "change"
            }
        };
    }

    #endregion Funding Eligibility History
}