using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Forms;

public class DeleteFormCommandHandler : IRequestHandler<DeleteFormCommand, BaseResponse<bool>>
{
    private readonly IGenericRepository<Form> _formRepository;

    public DeleteFormCommandHandler(IGenericRepository<Form> formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<BaseResponse<bool>> Handle(DeleteFormCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<bool>();

        try
        {
            var form = _formRepository.GetById(request.Id);
            if (form == null)
            {
                response.Data = false;
                response.Success = false;
                response.Message = $"Form with id '{form!.Id}' could not be found.";

                return response;
            }

            response.Data = _formRepository.Delete(request.Id);
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