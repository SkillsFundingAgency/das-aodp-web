using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Import
{
    public class GetJobRunsQuery : IRequest<BaseMediatrResponse<GetJobRunsQueryResponse>> 
    {
        public string JobName { get; set; }
    }

}
