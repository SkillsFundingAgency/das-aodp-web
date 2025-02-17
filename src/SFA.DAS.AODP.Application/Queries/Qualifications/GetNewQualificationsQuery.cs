using MediatR;
namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetNewQualificationsQuery : IRequest<BaseMediatrResponse<GetNewQualificationsQueryResponse>>
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }
}
