using System.Globalization;
using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover;

public class GetQualificationVersionsForRolloverQueryBuilderApiRequest(RolloverQueryBuilderRequest data) : IPostMultipartFormDataApiRequest
{
    public string PostUrl => "api/rollover/querybuilder/qualificationversions";

    public RolloverQueryBuilderRequest Data { get; } = data;

    public IEnumerable<KeyValuePair<string, string>> FormData =>
        Data.LevelIds.Select(id => FormValue(nameof(Data.LevelIds), id.ToString(CultureInfo.InvariantCulture)))
            .Concat(Data.TypeIds.Select(id => FormValue(nameof(Data.TypeIds), id.ToString(CultureInfo.InvariantCulture))))
            .Concat(Data.SectorSubjectAreaIds.Select(id => FormValue(nameof(Data.SectorSubjectAreaIds), id)))
            .Concat(Data.AwardingOrganisationIds.Select(id => FormValue(nameof(Data.AwardingOrganisationIds), id)));

    private static KeyValuePair<string, string> FormValue(string name, string value) => new(name, value);
}
