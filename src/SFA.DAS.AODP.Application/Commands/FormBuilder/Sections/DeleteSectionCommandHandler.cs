using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class DeleteSectionCommandHandler : IRequestHandler<DeleteSectionCommand, BaseResponse<bool>>
{
    private readonly IGenericRepository<Section> _sectionRepository;

    public DeleteSectionCommandHandler(IGenericRepository<Section> sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<BaseResponse<bool>> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<bool>();

        try
        {
            var section = _sectionRepository.GetById(request.Id);
            if (section == null)
            {
                response.Data = false;
                response.Success = false;
                response.Message = $"Section with id '{section!.Id}' could not be found.";

                return response;
            }

            response.Data = _sectionRepository.Delete(request.Id);
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
