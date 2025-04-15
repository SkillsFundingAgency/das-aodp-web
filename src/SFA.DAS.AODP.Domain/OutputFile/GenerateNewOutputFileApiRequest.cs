using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Domain.OutputFile;

public class GenerateNewOutputFileApiRequest : IPostApiRequest
{
    public string PostUrl => "api/outputfile/generate";
    public object Data { get; set; }
}
