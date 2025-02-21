using MediatR;

namespace SFA.DAS.AODP.Application.Queries.Application.Form;

public class GetFormPreviewByIdQuery : IRequest<BaseMediatrResponse<GetFormPreviewByIdQueryResponse>>
{
    public readonly Guid ApplicationId;
    public GetFormPreviewByIdQuery(Guid applicationId)
    {
        ApplicationId = applicationId;
    }
}