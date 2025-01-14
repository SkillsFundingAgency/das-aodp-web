using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

public class GetAllSectionsQueryHandler : IRequestHandler<GetAllSectionsQuery, BaseResponse<List<Section>>>
{
    private readonly IGenericRepository<Section> _sectionRepository;

    public GetAllSectionsQueryHandler(IGenericRepository<Section> sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<BaseResponse<List<Section>>> Handle(GetAllSectionsQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseResponse<List<Section>>();
        response.Success = false;
        try
        {
            response.Data = _sectionRepository.GetAll().Where(s => s.FormId == request.FormId).ToList();
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Message = ex.Message;
        }

        return response;
    }
}