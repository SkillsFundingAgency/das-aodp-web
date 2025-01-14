using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, BaseResponse<bool>>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public DeletePageCommandHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<BaseResponse<bool>> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<bool>();

        try
        {
            var page = _pageRepository.GetById(request.Id);
            if (page == null)
            {
                response.Data = false;
                response.Success = false;
                response.Message = $"Page with id '{page!.Id}' could not be found.";

                return response;
            }

            response.Data = _pageRepository.Delete(request.Id);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }

        return response;
    }
}
