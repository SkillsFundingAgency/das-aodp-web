using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetQualificationOutputFileQuery: IRequest<BaseMediatrResponse<GetQualificationOutputFileResponse>>
    {
        public string CurrentUsername { get; set; } = string.Empty;
        public DateTime PublicationDate { get; set; }

        public GetQualificationOutputFileQuery(string currentUsername, DateTime publicationDate)
        {
            CurrentUsername = currentUsername;
            PublicationDate = publicationDate;
        }
    }
}
