namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IPostMultipartJsonFileApiRequest
{
    string PostUrl { get; }
    object Data { get; }
}
