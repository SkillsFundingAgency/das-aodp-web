using MediatR;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class CreateFormCommandHandler : IRequestHandler<CreateFormCommand, CreateFormCommandResponse>
{
    private readonly IGenericRepository<Form> _formRepository;

    public CreateFormCommandHandler(IGenericRepository<Form> formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<CreateFormCommandResponse> Handle(CreateFormCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateFormCommandResponse
        {
            Success = false
        };

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


        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
