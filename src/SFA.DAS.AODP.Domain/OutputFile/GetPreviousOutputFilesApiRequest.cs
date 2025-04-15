using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Domain.OutputFile;

public class GetPreviousOutputFilesApiRequest : IGetApiRequest
{
    public string GetUrl => "api/outputfiles";
}
