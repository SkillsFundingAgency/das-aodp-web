using SFA.DAS.AODP.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.AODP.Domain.Files
{
    public class AuthoriseFileDownloadApiRequest : IGetApiRequest
    {
        public Guid FileId { get; set; }
        public string GetUrl => $"api/files/{FileId}/authorise";
    }
}
