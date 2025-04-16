using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Application.Queries.OutputFile;

public class GetOutputFileDownloadQueryResponse
{
    public Stream FileStream { get; set; }
    public string ContentType { get; set; } = string.Empty;
}
