using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, DeletePageCommandResponse>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public DeletePageCommandHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<DeletePageCommandResponse> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        var response = new DeletePageCommandResponse();

        try
        {
            var page = _pageRepository.GetById(request.Id);
            if (page == null)
            {
                response.Success = false;
                response.ErrorMessage = $"Page with id '{page!.Id}' could not be found.";

                return response;
            }

            _pageRepository.Delete(request.Id);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
