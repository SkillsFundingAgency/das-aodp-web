using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Handlers.FormBuilder.Pages;

public class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, GetAllPagesQueryResponse>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public GetAllPagesQueryHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<GetAllPagesQueryResponse> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        var response = new GetAllPagesQueryResponse();
        response.Success = false;
        try
        {
            response.Data = _pageRepository.GetAll().Where(p => p.SectionId == request.SectionId).ToList();
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
