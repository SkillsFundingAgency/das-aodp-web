using MediatR;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Test
{
    public class GetNewQualificationsQueryHandler : IRequestHandler<GetNewQualificationsQuery, BaseMediatrResponse<GetNewQualificationsQueryResponse>>
    {
        private readonly IApiClient _apiClient;

        public GetNewQualificationsQueryHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<GetNewQualificationsQueryResponse>> Handle(GetNewQualificationsQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<GetNewQualificationsQueryResponse>();
            response.Success = false;
            try
            {
                var result = await _apiClient.Get<GetNewQualificationsQueryResponse>(new GetNewQualificationsApiRequest()
                {
                     Skip = request.Skip,
                     Take = request.Take,
                     Name = request.Name,
                     Organisation = request.Organisation,
                     QAN = request.QAN,
                     ProcessStatusFilter = request.ProcessStatusFilter,
                });

                if (result != null)
                {
                    response.Value = result;
                    response.Success = true;
                }                
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
    }
}

