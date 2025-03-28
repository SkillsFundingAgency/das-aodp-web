namespace SFA.DAS.AODP.Application.Queries.Import
{
    public class GetJobResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public bool Enabled { get; set; }

        public string Status { get; set; } = null!;

        public DateTime? LastRunTime { get; set; }
    }   
}
