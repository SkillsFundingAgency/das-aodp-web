using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IGetAllApiRequest : IBaseApiRequest
{
    [JsonIgnore]
    string GetAllUrl { get; }
}