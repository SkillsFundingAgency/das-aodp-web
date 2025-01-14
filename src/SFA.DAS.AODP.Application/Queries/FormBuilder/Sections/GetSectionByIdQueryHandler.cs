using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, BaseResponse<Section>>
{
    private readonly IGenericRepository<Section> _sectionRepository;

    public GetSectionByIdQueryHandler(IGenericRepository<Section> sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<BaseResponse<Section>> Handle(GetSectionByIdQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<Section>();
        response.Success = false;
        try
        {
            response.Data = _sectionRepository.GetById(request.Id);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }
}
