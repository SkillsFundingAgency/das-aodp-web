using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Import 
{
    public class GetJobQuery : IRequest<BaseMediatrResponse<GetJobResponse>>
    {
        public string JobName { get; set; }
    }
}
