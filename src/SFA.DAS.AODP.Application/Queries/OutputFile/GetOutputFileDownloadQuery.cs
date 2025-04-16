using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Queries.OutputFile;

public class GetOutputFileDownloadQuery(string fileName) : IRequest<BaseMediatrResponse<GetOutputFileDownloadQueryResponse>> 
{ 
    public string FileName { get; set; } = fileName;
}
