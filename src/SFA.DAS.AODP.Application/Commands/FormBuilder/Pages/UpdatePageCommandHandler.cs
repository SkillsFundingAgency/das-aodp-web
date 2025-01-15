using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, UpdatePageCommandResponse>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public UpdatePageCommandHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<UpdatePageCommandResponse> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdatePageCommandResponse();
        response.Success = false;

        try
        {
            var page = _pageRepository.GetById(request.Id);

            if (page == null)
            {
                response.Success = false;
                response.ErrorMessage = $"Page with id '{page!.Id}' could not be found.";
                return response;
            }

            page.Title = request.Title;
            page.Description = request.Description;
            page.Order = request.Order;
            page.NextPageId = request.NextPageId;

            _pageRepository.Update(page);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
