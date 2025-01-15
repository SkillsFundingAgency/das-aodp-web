using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class UpdateFormCommandHandler : IRequestHandler<UpdateFormCommand, UpdateFormCommandResponse>
{
    private readonly IGenericRepository<Form> _formRepository;

    public UpdateFormCommandHandler(IGenericRepository<Form> formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<UpdateFormCommandResponse> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
    {
        var response = new UpdateFormCommandResponse();
        response.Success = false;

        try
        {
            var form = _formRepository.GetById(request.Id);

            if (form == null)
            {
                response.Success = false;
                response.ErrorMessage = $"Form with id '{form!.Id}' could not be found.";
                return response;
            }

            form.Name = request.Name;
            form.Version = request.Version;
            form.Published = request.Published;
            form.Key = request.Key;
            form.ApplicationTrackingTemplate = request.ApplicationTrackingTemplate;
            form.Order = request.Order;
            form.Description = request.Description;

            _formRepository.Update(form);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
