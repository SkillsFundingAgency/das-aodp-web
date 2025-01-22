using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IGetApiRequest 
{
    [JsonIgnore]
    string GetUrl { get; }
}
