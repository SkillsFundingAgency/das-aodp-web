using Newtonsoft.Json;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetNewQualificationsQueryResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonProperty("value")]
        public NewQualificationsData Value { get; set; } = new();
    }

    public class NewQualificationsData
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("newQualifications")]
        public List<NewQualification> NewQualifications { get; set; } = new();
    }

    public class NewQualification
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("awardingOrganisation")]
        public string? AwardingOrganisation { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }
    }
}


