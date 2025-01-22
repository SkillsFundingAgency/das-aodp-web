using MediatR;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

public class GetAllPagesQuery : IRequest<GetAllPagesQueryResponse>
{
    public readonly Guid SectionId;

    public GetAllPagesQuery(Guid sectionId)
    {
        SectionId = sectionId;
    }
}