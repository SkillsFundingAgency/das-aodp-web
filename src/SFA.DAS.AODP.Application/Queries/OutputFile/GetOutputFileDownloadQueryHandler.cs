using MediatR;
using SFA.DAS.AODP.Domain.OutputFile;
using SFA.DAS.AODP.Infrastructure.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Queries.OutputFile;

public class GetOutputFileDownloadQueryHandler : IRequestHandler<GetOutputFileDownloadQuery, BaseMediatrResponse<GetOutputFileDownloadQueryResponse>>
{
    private readonly IFileService _fileService;

    public GetOutputFileDownloadQueryHandler(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task<BaseMediatrResponse<GetOutputFileDownloadQueryResponse>> Handle(GetOutputFileDownloadQuery request, CancellationToken cancellationToken)
    {
        var response = new BaseMediatrResponse<GetOutputFileDownloadQueryResponse>
        {
            Success = false
        };

        try
        {
            var fileStream = await _fileService.OpenReadStreamAsync(request.FileName);
            var blobContentType = _fileService.GetBlobContentType(request.FileName);
            response.Value = new GetOutputFileDownloadQueryResponse
            {
                FileStream = fileStream,
                ContentType = blobContentType
            };
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = ex.Message;
            response.Success = false;
        }

        return response;
    }
}
