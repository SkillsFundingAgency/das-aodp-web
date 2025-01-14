using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class CreateFormCommandHandler : IRequestHandler<CreateFormCommand, BaseResponse<Form>>
{
    private readonly IGenericRepository<Form> _formRepository;

    public CreateFormCommandHandler(IGenericRepository<Form> formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<BaseResponse<Form>> Handle(CreateFormCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<Form>();
        try
        {
            var form = new Form
            {
                Name = request.Name,
                Version = request.Version,
                Published = request.Published,
                Key = request.Key,
                ApplicationTrackingTemplate = request.ApplicationTrackingTemplate,
                Order = request.Order,
                Description = request.Description,
            };

            _formRepository.Add(form);

            response.Data = form;
            response.Success = true;
            response.Message = "Form added.";
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }

        return response;
    }
}
