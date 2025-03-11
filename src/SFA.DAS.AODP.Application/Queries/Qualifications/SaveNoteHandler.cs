using MediatR;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Qualifications.Requests;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{

    public class SaveNoteHandler : IRequestHandler<SaveNoteQuery, BaseMediatrResponse<SaveNoteResponse>>
    {
        private readonly IApiClient _apiClient;

        public SaveNoteHandler(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseMediatrResponse<SaveNoteResponse>> Handle(SaveNoteQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseMediatrResponse<SaveNoteResponse>();
            try
            {
                var result = await _apiClient.Get<BaseMediatrResponse<SaveNoteResponse>>(new SaveNotesApiRequest() { ActionTypeId=request.ActionTypeId,QualificationReferenceId=request.QualificationReferenceId,Notes=request.Notes});
                if (result != null)
                {
                    response.Value = result.Value;
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = $"Error with Save";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }

            return response;
        }
    }
}


