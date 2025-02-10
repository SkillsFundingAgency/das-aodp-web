using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SFA.DAS.AODP.Authentication.DfeSignInApi.JWTHelpers
{
    public sealed class TokenDataSerializer : ITokenDataSerializer
    {
        private readonly JsonSerializer _serializer;

        public TokenDataSerializer() : this(JsonSerializer.CreateDefault()) { }

        public TokenDataSerializer(JsonSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException("serializer");
        }

        public string Serialize(object obj) =>
            JObject.FromObject(obj, _serializer).ToString(_serializer.Formatting, _serializer.Converters.ToArray());
    }

    public interface ITokenDataSerializer
    {
        string Serialize(object obj);
    }
}
