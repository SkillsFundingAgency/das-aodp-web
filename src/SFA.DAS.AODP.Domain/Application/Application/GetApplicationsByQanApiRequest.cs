using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Application.Application;

public class GetApplicationsByQanApiRequest : IGetApiRequest
{
    public string? Qan { get; set; }
    public string GetUrl => $"api/applications/qualifications/{Qan}";
}
