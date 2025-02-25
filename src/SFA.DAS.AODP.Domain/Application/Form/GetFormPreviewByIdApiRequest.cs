using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Form;
public class GetFormPreviewByIdApiRequest : IGetApiRequest
{
    public Guid _applicationId { get; set; }
    public GetFormPreviewByIdApiRequest(Guid applicationId)
    {
        _applicationId = applicationId;
    }

    public string GetUrl => $"api/applications/{_applicationId}/form-preview";
}