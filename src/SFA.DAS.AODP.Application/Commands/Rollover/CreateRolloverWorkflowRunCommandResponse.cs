namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    public class CreateRolloverWorkflowRunCommandResponse
    {
        public Guid RolloverWorkflowRunId { get; set; }
    }

    public class RolloverWorkflowRun
    {
        public Guid WorkflowRunId { get; set; }
        public DateTime? FundingEndDateEligibilityThreshold { get; set; }
        public DateTime? OperationalEndDateEligibilityThreshold { get; set; }
        public DateTime? MaximumApprovalFundingEndDate { get; set; }
    }
}