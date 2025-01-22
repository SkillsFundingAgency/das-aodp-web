using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IPatchApiRequest<TData> 
{
    [JsonIgnore]
    string PatchUrl { get; }

    TData Data { get; set; }
}