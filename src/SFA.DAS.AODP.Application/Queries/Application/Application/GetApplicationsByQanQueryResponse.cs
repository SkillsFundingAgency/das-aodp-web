using SFA.DAS.AODP.Models.Application;

namespace SFA.DAS.AODP.Application.Queries.Application.Application;

public class GetApplicationsByQanQueryResponse
{
    public List<Application> Applications { get; set; } = new();

    public class Application
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public ApplicationStatus Status { get; set; }
        public int ReferenceId { get; set; }
        public Guid? ApplicationReviewId { get; set; }
    }
}

