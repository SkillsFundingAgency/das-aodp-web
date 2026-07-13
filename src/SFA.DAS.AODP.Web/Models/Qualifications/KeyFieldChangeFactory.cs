using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Extensions;

namespace SFA.DAS.AODP.Web.Models.Qualifications;

public static class KeyFieldChangeFactory
{
    private static string FormatValue(object? value)
    {
        return value?.ToString() ?? string.Empty;
    }

    private static string FormatDate(DateTime? date)
    {
        if (date.HasValue)
        {
            return date.Value.ToStandard12HourDateTimeFormat();
        }

        return string.Empty;
    }

    public static KeyFieldChanges? Create(KeyField keyField, ChangedQualificationDetailsViewModel latest, ChangedQualificationDetailsViewModel previous)
    {
        if (keyField is null)
        {
            return null;
        }

        if (latest is null)
        {
            return null;
        }

        if (previous is null)
        {
            return null;
        }

        return keyField.Key switch
        {
            _ when keyField.Matches(KeyField.OrganisationName.Key) => new KeyFieldChanges(KeyField.OrganisationName.DisplayName, FormatValue(previous.Organisation.NameOfqual), FormatValue(latest.Organisation.NameOfqual)),
            _ when keyField.Matches(KeyField.Title.Key) => new KeyFieldChanges(KeyField.Title.DisplayName, FormatValue(previous.Name), FormatValue(latest.Name)),
            _ when keyField.Matches(KeyField.Level.Key) => new KeyFieldChanges(KeyField.Level.DisplayName, FormatValue(previous.Level), FormatValue(latest.Level)),
            _ when keyField.Matches(KeyField.Type.Key) => new KeyFieldChanges(KeyField.Type.DisplayName, FormatValue(previous.Type), FormatValue(latest.Type)),
            _ when keyField.Matches(KeyField.TotalCredits.Key) => new KeyFieldChanges(KeyField.TotalCredits.DisplayName, FormatValue(previous.TotalCredits?.ToString()), FormatValue(latest.TotalCredits?.ToString())),
            _ when keyField.Matches(KeyField.Ssa.Key) => new KeyFieldChanges(KeyField.Ssa.DisplayName, FormatValue(previous.Ssa), FormatValue(latest.Ssa)),
            _ when keyField.Matches(KeyField.GradingType.Key) => new KeyFieldChanges(KeyField.GradingType.DisplayName, FormatValue(previous.GradingType), FormatValue(latest.GradingType)),
            _ when keyField.Matches(KeyField.OfferedInEngland.Key) => new KeyFieldChanges(KeyField.OfferedInEngland.DisplayName, FormatValue(previous.OfferedInEngland.ToString()), FormatValue(latest.OfferedInEngland.ToString())),
            _ when keyField.Matches(KeyField.PreSixteen.Key) => new KeyFieldChanges(KeyField.PreSixteen.DisplayName, FormatValue(previous.PreSixteen?.ToString()), FormatValue(latest.PreSixteen?.ToString())),
            _ when keyField.Matches(KeyField.SixteenToEighteen.Key) => new KeyFieldChanges(KeyField.SixteenToEighteen.DisplayName, FormatValue(previous.SixteenToEighteen?.ToString()), FormatValue(latest.SixteenToEighteen?.ToString())),
            _ when keyField.Matches(KeyField.EighteenPlus.Key) => new KeyFieldChanges(KeyField.EighteenPlus.DisplayName, FormatValue(previous.EighteenPlus?.ToString()), FormatValue(latest.EighteenPlus?.ToString())),
            _ when keyField.Matches(KeyField.NineteenPlus.Key) => new KeyFieldChanges(KeyField.NineteenPlus.DisplayName, FormatValue(previous.NineteenPlus?.ToString()), FormatValue(latest.NineteenPlus?.ToString())),
            _ when keyField.Matches(KeyField.IntentionToSeekFundingInEngland.Key) => new KeyFieldChanges(KeyField.IntentionToSeekFundingInEngland.DisplayName, FormatValue(previous.IntentionToSeekFundingInEngland?.ToString()), FormatValue(latest.IntentionToSeekFundingInEngland?.ToString())),
            _ when keyField.Matches(KeyField.GLH.Key) => new KeyFieldChanges(KeyField.GLH.DisplayName, FormatValue(previous.Glh?.ToString()), FormatValue(latest.Glh?.ToString())),
            _ when keyField.Matches(KeyField.MinimumGlh.Key) => new KeyFieldChanges(KeyField.MinimumGlh.DisplayName, FormatValue(previous.MinimumGlh?.ToString()), FormatValue(latest.MinimumGlh?.ToString())),
            _ when keyField.Matches(KeyField.Tqt.Key) => new KeyFieldChanges(KeyField.Tqt.DisplayName, FormatValue(previous.Tqt?.ToString()), FormatValue(latest.Tqt?.ToString())),
            _ when keyField.Matches(KeyField.OperationalEndDate.Key) => new KeyFieldChanges(KeyField.OperationalEndDate.DisplayName, FormatDate(previous.OperationalEndDate), FormatDate(latest.OperationalEndDate)),
            _ when keyField.Matches(KeyField.OfferedInternationally.Key) => new KeyFieldChanges(KeyField.OfferedInternationally.DisplayName, FormatValue(previous.OfferedInternationally?.ToString()), FormatValue(latest.OfferedInternationally?.ToString())),
            _ => null
        };
    }
}
