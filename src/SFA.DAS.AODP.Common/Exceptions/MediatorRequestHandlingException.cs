namespace SFA.DAS.AODP.Common.Exceptions
{
    public class MediatorRequestHandlingException : ApplicationException
    {
        public MediatorRequestHandlingException()
        {
        }

        public MediatorRequestHandlingException(string message) : base(message)
        {
        }

        public MediatorRequestHandlingException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}