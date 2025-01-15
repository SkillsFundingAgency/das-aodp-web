using MediatR;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, CreatePageCommandResponse>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public CreatePageCommandHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<CreatePageCommandResponse> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        var response = new CreatePageCommandResponse();
        try
        {
            var page = new Page
            {
                SectionId = request.SectionId,
                Title = request.Title,
                Description = request.Description,
                Order = request.Order,
                NextPageId = request.NextPageId
            };

            _pageRepository.Add(page);

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
