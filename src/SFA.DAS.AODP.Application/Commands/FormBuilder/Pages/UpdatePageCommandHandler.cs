using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, BaseResponse<bool>>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public UpdatePageCommandHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<BaseResponse<bool>> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<bool>();
        response.Success = false;

        try
        {
            var page = _pageRepository.GetById(request.Id);

            if (page == null)
            {
                response.Success = false;
                response.Message = $"Page with id '{page!.Id}' could not be found.";
                return response;
            }

            page.Title = request.Title;
            page.Description = request.Description;
            page.Order = request.Order;
            page.NextPageId = request.NextPageId;

            response.Data = _pageRepository.Update(page);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }
}
