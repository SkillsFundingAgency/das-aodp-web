using SFA.DAS.FAA.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Forms.GetAllForms
{
    public class GetAllFormsRequest : IGetApiRequest
    {
        public string GetUrl => "/api/forms";
    }
}
