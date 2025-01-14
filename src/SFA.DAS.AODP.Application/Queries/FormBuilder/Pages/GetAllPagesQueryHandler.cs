using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Handlers.FormBuilder.Pages;

public class GetAllPagesQueryHandler : IRequestHandler<GetAllPagesQuery, BaseResponse<List<Page>>>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public GetAllPagesQueryHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<BaseResponse<List<Page>>> Handle(GetAllPagesQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<List<Page>>();
        response.Success = false;
        try
        {
            response.Data = _pageRepository.GetAll().Where(p => p.SectionId == request.SectionId).ToList();
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }
}
