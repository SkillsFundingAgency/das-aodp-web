using Newtonsoft.Json;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationDetailsQueryResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonProperty("value")]
        public QualificationDetail Value { get; set; } = new();
    }

    public class QualificationDetail
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("priority")]
        public string? Priority { get; set; }

        [JsonProperty("changes")]
        public string? Changes { get; set; }

        [JsonProperty("qualificationReference")]
        public string? QualificationReference { get; set; }

        [JsonProperty("awardingOrganisation")]
        public string? AwardingOrganisation { get; set; }

        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("qualificationType")]
        public string? QualificationType { get; set; }

        [JsonProperty("level")]
        public string? Level { get; set; }

        [JsonProperty("proposedChanges")]
        public string? ProposedChanges { get; set; }

        [JsonProperty("ageGroup")]
        public string? AgeGroup { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("subject")]
        public string? Subject { get; set; }

        [JsonProperty("sectorSubjectArea")]
        public string? SectorSubjectArea { get; set; }

        [JsonProperty("comments")]
        public string? Comments { get; set; }
    }
}
