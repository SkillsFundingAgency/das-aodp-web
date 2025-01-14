using Newtonsoft.Json;

namespace SFA.DAS.AODP.Data
{
    public class PaginatedResult<T>
    {
        [JsonProperty("results")]
        public required List<T> Results { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

    }
}
