using MediatR;

namespace SFA.DAS.ADPO.Application.Queries.Test
{
    public class TestQuery : IRequest<TestQueryResponse>
    {
        public int Id { get; set; }
    }
}
