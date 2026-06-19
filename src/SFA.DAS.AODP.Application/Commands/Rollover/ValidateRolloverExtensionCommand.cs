using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    [ExcludeFromCodeCoverage]
    public class ValidateRolloverExtensionCommand : IRequest<BaseMediatrResponse<ValidateRolloverExtensionCommandResponse>>
    {
        public List<RolloverCandidateForValidation> RolloverCandidates { get; set; } = new();
    }

    [ExcludeFromCodeCoverage]
    public class RolloverCandidateForValidation
    {
        public string? Qan { get; set; }
        public string FundingStreamName { get; set; } = string.Empty;
        public string RollOverStatus { get; set; } = string.Empty;
        public string? ExclusionReason { get; set; }
        public DateTime? ProposedFundingApprovalEndDate { get; set; }
        public string? Comments { get; init; }
    }
}