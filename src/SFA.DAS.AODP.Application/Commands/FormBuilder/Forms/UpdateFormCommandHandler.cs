using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class UpdateFormCommandHandler : IRequestHandler<UpdateFormCommand, BaseResponse<bool>>
{
    private readonly IGenericRepository<Form> _formRepository;

    public UpdateFormCommandHandler(IGenericRepository<Form> formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<BaseResponse<bool>> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<bool>();
        response.Success = false;

        try
        {
            var form = _formRepository.GetById(request.Id);

            if (form == null)
            {
                response.Success = false;
                response.Message = $"Form with id '{form!.Id}' could not be found.";
                return response;
            }

            form.Name = request.Name;
            form.Version = request.Version;
            form.Published = request.Published;
            form.Key = request.Key;
            form.ApplicationTrackingTemplate = request.ApplicationTrackingTemplate;
            form.Order = request.Order;
            form.Description = request.Description;

            response.Data = _formRepository.Update(form);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }
}
