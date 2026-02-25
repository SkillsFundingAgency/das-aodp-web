using SFA.DAS.AODP.Models.Common;

namespace SFA.DAS.AODP.Models.Exceptions
{
    public class FileUploadPolicyException : Exception
    {
        public FileUploadRejectionReason Reason { get; }

        public FileUploadPolicyException(FileUploadRejectionReason reason)
        {
            Reason = reason;
        }
    }
}
