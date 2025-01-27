using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IDeleteApiRequest 
{
    [JsonIgnore]
    string DeleteUrl { get; }
}