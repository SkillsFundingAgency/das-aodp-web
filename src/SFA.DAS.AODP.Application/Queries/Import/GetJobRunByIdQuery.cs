﻿using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Import
{
    public class GetJobRunByIdQuery : IRequest<BaseMediatrResponse<GetJobRunByIdQueryResponse>>
    {
        public Guid Id { get; }

        public GetJobRunByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
