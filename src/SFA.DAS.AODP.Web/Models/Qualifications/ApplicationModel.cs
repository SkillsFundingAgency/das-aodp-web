using SFA.DAS.AODP.Application.Queries.Application.Application;
using SFA.DAS.AODP.Models.Application;

namespace SFA.DAS.AODP.Web.Models.Qualifications;

public class ApplicationModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string? Status { get; set; }
    public int ReferenceId { get; set; }
    public DateTime? ApplicationDate => SubmittedDate ?? CreatedDate;
    public Guid? ApplicationReviewId { get; set; }
}