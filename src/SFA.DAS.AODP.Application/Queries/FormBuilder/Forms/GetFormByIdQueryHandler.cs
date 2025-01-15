using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetFormByIdQueryHandler : IRequestHandler<GetFormByIdQuery, GetFormByIdQueryResponse>
{
    private readonly IGenericRepository<Form> _formRepository;

    public GetFormByIdQueryHandler(IGenericRepository<Form> formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<GetFormByIdQueryResponse> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new GetFormByIdQueryResponse();
        response.Success = false;
        try
        {
            response.Data = _formRepository.GetById(request.Id);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}
