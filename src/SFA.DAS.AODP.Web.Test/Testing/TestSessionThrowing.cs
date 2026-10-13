using Microsoft.AspNetCore.Http;

namespace SFA.DAS.AODP.Web.UnitTests.Testing
{
    public class TestSessionThrowing : ISession
    {
        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => Array.Empty<string>();

        public void Clear()
        {
            throw new Exception("Session Clear failed");
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new Exception("Session Commit failed");
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            throw new Exception("Session Load failed");
        }

        public void Remove(string key)
        {
            throw new Exception("Session Remove failed");
        }

        public void Set(string key, byte[] value)
        {
            throw new Exception("Session Set failed");
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            throw new Exception("Session Get failed");
        }

        public void SetString(string key, string value)
        {
            throw new Exception("Session SetString failed");
        }

        public string? GetString(string key)
        {
            throw new Exception("Session GetString failed");
        }
    }

}
