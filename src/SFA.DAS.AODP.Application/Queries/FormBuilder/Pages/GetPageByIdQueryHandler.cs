using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Handlers.FormBuilder.Pages;

public class GetPageByIdQueryHandler : IRequestHandler<GetPageByIdQuery, BaseResponse<Page>>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public GetPageByIdQueryHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<BaseResponse<Page>> Handle(GetPageByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<Page>();
        response.Success = false;
        try
        {
            response.Data = _pageRepository.GetById(request.Id);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }
}
