using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand, UpdateSectionCommandResponse>
{
    private readonly IGenericRepository<Section> _sectionRepository;

    public UpdateSectionCommandHandler(IGenericRepository<Section> sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<UpdateSectionCommandResponse> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdateSectionCommandResponse();
        response.Success = false;

        try
        {
            var section = _sectionRepository.GetById(request.Id);

            if (section == null)
            {
                response.Success = false;
                response.ErrorMessage = $"Section with id '{section!.Id}' could not be found.";
                return response;
            }

            section.Order = request.Order;
            section.Title = request.Title;
            section.Description = request.Description;
            section.NextSectionId = request.NextSectionId;

            _sectionRepository.Update(section);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
