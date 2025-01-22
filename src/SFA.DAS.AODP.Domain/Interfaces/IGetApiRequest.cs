using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IGetApiRequest : IBaseApiRequest
{
    [JsonIgnore]
    string GetUrl { get; }
}
