using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Handlers.FormBuilder.Pages;

public class GetPageByIdQueryHandler : IRequestHandler<GetPageByIdQuery, GetPageByIdQueryResponse>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public GetPageByIdQueryHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<GetPageByIdQueryResponse> Handle(GetPageByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new GetPageByIdQueryResponse();
        response.Success = false;
        try
        {
            response.Data = _pageRepository.GetById(request.Id);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
