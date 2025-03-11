using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class SaveNoteQuery : IRequest<BaseMediatrResponse<SaveNoteResponse>>
    {

        public Guid QualificationReferenceId { get; set; }
        public Guid ActionTypeId { get; set; }
        public string Notes { get; set; }
    }
}
