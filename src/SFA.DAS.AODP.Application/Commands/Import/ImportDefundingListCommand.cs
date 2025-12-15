using MediatR;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class ImportDefundingListCommand : IRequest<BaseMediatrResponse<ImportDefundingListCommandResponse>>
{
    public IFormFile? File { get; set; }
    public string? FileName { get; set; }
}
