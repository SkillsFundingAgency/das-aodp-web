using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, BaseResponse<Section>>
{
    private readonly IGenericRepository<Section> _sectionRepository;

    public CreateSectionCommandHandler(IGenericRepository<Section> sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<BaseResponse<Section>> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<Section>();
        try
        {
            var section = new Section
            {
                FormId = request.FormId,
                Order = request.Order,
                Title = request.Title,
                Description = request.Description,
                NextSectionId = request.NextSectionId,
            };

            _sectionRepository.Add(section);

            response.Data = section;
            response.Success = true;
            response.Message = "Section added.";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }

        return response;
    }
}
