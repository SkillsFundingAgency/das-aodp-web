using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.Rollover
{
    public class CreateRolloverWorkflowRunApiRequest : IPostApiRequest
    {
        public CreateRolloverWorkflowRunApiRequest(object data)
        {
            Data = data;
        }

        public object Data { get; set; }

        public string PostUrl => $"api/rollover/rolloverworkflowruns";
    }
}