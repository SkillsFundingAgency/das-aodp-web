namespace SFA.DAS.AODP.Domain.Interfaces;

public interface IPostMultipartFormDataApiRequest
{
    string PostUrl { get; }
    IEnumerable<KeyValuePair<string, string>> FormData { get; }
}
