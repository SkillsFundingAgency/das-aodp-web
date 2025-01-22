using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IBaseApiRequest
{
    [JsonIgnore]
    string Version => "1.0";
}
