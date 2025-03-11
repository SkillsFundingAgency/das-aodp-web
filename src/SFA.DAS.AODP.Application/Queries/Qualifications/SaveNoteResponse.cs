using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class SaveNoteResponse : IRequest<BaseMediatrResponse<GetChangedQualificationDetailsResponse>>
    {
        public string QualificationReference { get; set; }
    }
}
