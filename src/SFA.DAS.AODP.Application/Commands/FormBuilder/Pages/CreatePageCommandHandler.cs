using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Pages;

public class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, BaseResponse<Page>>
{
    private readonly IGenericRepository<Page> _pageRepository;

    public CreatePageCommandHandler(IGenericRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<BaseResponse<Page>> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<Page>();
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

            response.Data = page;
            response.Success = true;
            response.Message = "Page added.";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }

        return response;
    }
}
