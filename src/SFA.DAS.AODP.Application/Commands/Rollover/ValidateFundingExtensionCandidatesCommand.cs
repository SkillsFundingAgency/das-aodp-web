using MediatR;
using SFA.DAS.AODP.Models.Rollover;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AODP.Application.Commands.Rollover
{
    [ExcludeFromCodeCoverage]
    public class ValidateFundingExtensionCandidatesCommand : IRequest<BaseMediatrResponse<ValidateFundingExtensionCandidatesCommandResponse>>
    {
        public List<FundingExtensionCandidate> FundingExtensionCandidates { get; set; } = new();
    }
}