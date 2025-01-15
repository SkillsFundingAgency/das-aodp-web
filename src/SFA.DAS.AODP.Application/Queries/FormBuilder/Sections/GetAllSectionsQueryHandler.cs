using MediatR;
using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Application.Repository;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

public class GetAllSectionsQueryHandler : IRequestHandler<GetAllSectionsQuery, GetAllSectionsQueryResponse>
{
    private readonly IGenericRepository<Section> _sectionRepository;

    public GetAllSectionsQueryHandler(IGenericRepository<Section> sectionRepository)
    {
        _sectionRepository = sectionRepository;
    }

    public async Task<GetAllSectionsQueryResponse> Handle(GetAllSectionsQuery request, CancellationToken cancellationToken)
    {
        var response = new GetAllSectionsQueryResponse();
        response.Success = false;
        try
        {
            response.Data = _sectionRepository.GetAll().Where(s => s.FormId == request.FormId).ToList();
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
        }

        return response;
    }
}