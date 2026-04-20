using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Qualifications;
using ActionType = SFA.DAS.AODP.Application.Queries.Qualifications.ActionType;

namespace SFA.DAS.AODP.Application.Services
{
    //Builds additional timeline entries to describe
    // - changed field values (if VersionFieldChanges contains field names)
    // - eligibility failures (if FundingEligibilityFailedFields contains field names)
    //
    //
    public class QualificationTimelineHistoryBuilder : IQualificationTimelineHistoryBuilder
    {
        private const string DateTimeFormat = "MM/dd/yy HH:mm";

        public List<QualificationDiscussionHistory> BuildTimelineEntries(List<GetQualificationDetailsQueryResponse> versions)
        {
            var entries = new List<QualificationDiscussionHistory>();

            entries.AddRange(BuildFundingEligibilityEntries(versions));
            entries.AddRange(BuildFieldChangeEntries(versions));

            return entries;
        }

        public List<FieldChange> GetKeyFieldChanges(
        GetQualificationDetailsQueryResponse latestVersion,
        GetQualificationDetailsQueryResponse previousVersion)
        {
            var fields =  latestVersion.VersionFieldChanges?.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
                ?? [];

            var changes = new List<FieldChange>();

            foreach (var item in fields)
            {
                switch (item)
                {
                    case "EligibleForFunding":
                        changes.Add(new FieldChange(
                            "eligible for funding",
                            previousVersion.EligibleForFunding?.ToString(),
                            latestVersion.EligibleForFunding?.ToString()));
                        break;

                    case "OrganisationName":
                        changes.Add(new FieldChange(
                            "organisation name",
                            previousVersion.Organisation?.NameOfqual,
                            latestVersion.Organisation?.NameOfqual));
                        break;

                    case "Title":
                        changes.Add(new FieldChange(
                            "title",
                            previousVersion.Qual?.QualificationName,
                            latestVersion.Qual?.QualificationName));
                        break;

                    case "Level":
                        changes.Add(new FieldChange(
                            "level",
                            previousVersion.Level?.ToString(),
                            latestVersion.Level?.ToString()));
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
                            previousVersion.TotalCredits?.ToString(),
                            latestVersion.TotalCredits?.ToString()));
                        break;

                    case "Ssa":
                        changes.Add(new FieldChange(
                            "SSA",
                            previousVersion.Ssa?.ToString(),
                            latestVersion.Ssa?.ToString()));
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
                            previousVersion.IntentionToSeekFundingInEngland?.ToString(),
                            latestVersion.IntentionToSeekFundingInEngland?.ToString()));
                        break;

                    case "PreSixteen":
                        changes.Add(new FieldChange(
                            "pre-sixteen",
                            previousVersion.PreSixteen?.ToString(),
                            latestVersion.PreSixteen?.ToString()));
                        break;

                    case "SixteenToEighteen":
                        changes.Add(new FieldChange(
                            "sixteen to eighteen",
                            previousVersion.SixteenToEighteen?.ToString(),
                            latestVersion.SixteenToEighteen?.ToString()));
                        break;

                    case "EighteenPlus":
                        changes.Add(new FieldChange(
                            "eighteen plus",
                            previousVersion.EighteenPlus?.ToString(),
                            latestVersion.EighteenPlus?.ToString()));
                        break;

                    case "NineteenPlus":
                        changes.Add(new FieldChange(
                            "nineteen plus",
                            previousVersion.NineteenPlus?.ToString(),
                            latestVersion.NineteenPlus?.ToString()));
                        break;

                    case "Glh":
                        changes.Add(new FieldChange(
                            "guided learning hours (GLH)",
                            previousVersion.Glh?.ToString(),
                            latestVersion.Glh?.ToString()));
                        break;

                    case "MinimumGlh":
                        changes.Add(new FieldChange(
                            "minimum glh",
                            previousVersion.MinimumGlh?.ToString(),
                            latestVersion.MinimumGlh?.ToString()));
                        break;

                    case "Tqt":
                        changes.Add(new FieldChange(
                            "total qualification time (TQT)",
                            previousVersion.Tqt?.ToString(),
                            latestVersion.Tqt?.ToString()));
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
                            previousVersion.Version?.ToString(),
                            latestVersion.Version?.ToString()));
                        break;

                    case "OfferedInternationally":
                        changes.Add(new FieldChange(
                            "offered internationally",
                            previousVersion.OfferedInternationally?.ToString(),
                            latestVersion.OfferedInternationally?.ToString()));
                        break;
                }
            }

            return changes;
        }

        private List<QualificationDiscussionHistory> BuildFieldChangeEntries(List<GetQualificationDetailsQueryResponse> versions)
        {
            var history = new List<QualificationDiscussionHistory>();

            if (versions.Count == 0)
            {
                return history;
            }

            var latestVersionNumber = versions.Max(i => i.Version) ?? 0;
            var currentVersion = versions.FirstOrDefault(v => v.Version == latestVersionNumber);

            if (latestVersionNumber <= 1 || currentVersion == null)
            {
                return history;
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
                    history.Add(changeEntry);
                }
            }

            return history;
        }

        private QualificationDiscussionHistory? BuildChangeHistoryEntry(
            GetQualificationDetailsQueryResponse latestVersion,
            GetQualificationDetailsQueryResponse previousVersion)
        {
            var keyFieldChanges = GetKeyFieldChanges(latestVersion, previousVersion);

            if (keyFieldChanges.Count == 0)
            {
                return null;
            }

            return new QualificationDiscussionHistory
            {
                Notes = BuildChangeString(keyFieldChanges),
                Title = "Change",
                UserDisplayName = "OFQUAL Import",
                Timestamp = latestVersion.InsertedTimestamp,
                ActionType = new ActionType
                {
                    Id = Guid.Empty,
                    Description = "change"
                }
            };
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

        private static List<QualificationDiscussionHistory> BuildFundingEligibilityEntries(List<GetQualificationDetailsQueryResponse> versions)
        {
            var history = new List<QualificationDiscussionHistory>();

            foreach (var version in versions)
            {
                var fundingEntry = BuildFundingEligibilityHistoryEntry(version);

                if (fundingEntry != null)
                {
                    history.Add(fundingEntry);
                }
            }

            return history;
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
                qualificationVersion.FundingEligibilityFailedFields);

            var fundingNotes = fundingStatus.ToDisplayString();

            if (string.IsNullOrWhiteSpace(fundingNotes))
            {
                return null;
            }

            return new QualificationDiscussionHistory
            {
                Notes = fundingNotes,
                Title = "Change",
                UserDisplayName = "OFQUAL Import",
                Timestamp = qualificationVersion.InsertedTimestamp,
                ActionType = new Queries.Qualifications.ActionType
                {
                    Id = Guid.Empty,
                    Description = "change"
                }
            };
        }
    }
}
