using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetFormByIdQueryHandler : IRequestHandler<GetFormByIdQuery, BaseResponse<Form>>
{
    private readonly IGenericRepository<Form> _formRepository;

    public GetFormByIdQueryHandler(IGenericRepository<Form> formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<BaseResponse<Form>> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<Form?>();
        response.Success = false;
        try
        {
            response.Data = _formRepository.GetById(request.Id);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }
}
