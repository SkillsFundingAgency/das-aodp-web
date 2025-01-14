using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand, BaseResponse<bool>>
{
    private readonly IGenericRepository<Section> _sectionRepository;

    public UpdateSectionCommandHandler(IGenericRepository<Section> sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<BaseResponse<bool>> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<bool>();
        response.Success = false;

        try
        {
            var section = _sectionRepository.GetById(request.Id);

            if (section == null)
            {
                response.Success = false;
                response.Message = $"Section with id '{section!.Id}' could not be found.";
                return response;
            }

            section.Order = request.Order;
            section.Title = request.Title;
            section.Description = request.Description;
            section.NextSectionId = request.NextSectionId;

            response.Data = _sectionRepository.Update(section);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }
}
