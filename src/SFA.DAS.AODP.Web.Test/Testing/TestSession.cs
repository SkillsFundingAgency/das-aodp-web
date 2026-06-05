using Microsoft.AspNetCore.Http;

namespace SFA.DAS.AODP.Web.UnitTests.Testing
{
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    public class ThrowingSession : ISession
    {
        private readonly bool _throwOnGet;
        private readonly bool _throwOnSet;

        public ThrowingSession(bool throwOnGet, bool throwOnSet)
        {
            _throwOnGet = throwOnGet;
            _throwOnSet = throwOnSet;
        }

        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => Array.Empty<string>();

        public void Clear()
        {
            if (_throwOnSet) throw new Exception("Clear failed");
        }

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key)
        {
            if (_throwOnSet) throw new Exception("Remove failed");
        }

        public void Set(string key, byte[] value)
        {
            if (_throwOnSet) throw new Exception("Set failed");
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_throwOnGet) throw new Exception("Get failed");
            value = Array.Empty<byte>();
            return false;
        }
    }
}
