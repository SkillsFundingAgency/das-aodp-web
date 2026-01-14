using MediatR;
using SFA.DAS.AODP.Application;

public class CreateApplicationCommand : IRequest<BaseMediatrResponse<CreateApplicationCommandResponse>>
{
    //Remove comments after testing
    public string Title { get; set; }
    public string Owner { get; set; }
    public Guid FormVersionId { get; set; }
    public string? QualificationNumber { get; set; }
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; }
    public string OrganisationUkprn { get; set; }
}
