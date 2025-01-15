using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class DeleteFormCommandHandler : IRequestHandler<DeleteFormCommand, DeleteFormCommandResponse>
{
    private readonly IGenericRepository<Form> _formRepository;

    public DeleteFormCommandHandler(IGenericRepository<Form> formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<DeleteFormCommandResponse> Handle(DeleteFormCommand request, CancellationToken cancellationToken)
    {
        var response = new DeleteFormCommandResponse();
        try
        {
            var form = _formRepository.GetById(request.Id);
            if (form == null)
            {
                response.Success = false;
                response.ErrorMessage = $"Form with id '{form!.Id}' could not be found.";

                return response;
            }

            _formRepository.Delete(request.Id);
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