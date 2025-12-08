using MediatR;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.AODP.Application.Commands.Import;

public class ImportDefundingListCommand : IRequest<BaseMediatrResponse<ImportDefundingListResponse>>
{
    public IFormFile? File { get; set; }
}