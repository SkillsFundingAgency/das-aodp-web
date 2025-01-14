using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetAllFormsQueryHandler : IRequestHandler<GetAllFormsQuery, BaseResponse<List<Form>>>
{
    private readonly IGenericRepository<Form> _formRepository;

    public GetAllFormsQueryHandler(IGenericRepository<Form> formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<BaseResponse<List<Form>>> Handle(GetAllFormsQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<List<Form>>();
        response.Success = false;
        try
        {
            response.Data = _formRepository.GetAll().ToList();
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }
}