using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Authentication.Configuration;
using SFA.DAS.AODP.Authentication.Constants;

namespace SFA.DAS.AODP.Authentication.DfeSignInApi.JWTHelpers
{
    public interface ITokenBuilder
    {
        string CreateToken();
    }
    public class TokenBuilder : ITokenBuilder
    {
        private byte[] SecretKey { get; set; }
        private readonly ITokenDataSerializer _tokenDataSerializer;
        private readonly TokenData _tokenData;

        public TokenBuilder(ITokenDataSerializer tokenDataSerializer, IOptions<DfEOidcConfiguration> configuration)
        {
            _tokenDataSerializer = tokenDataSerializer;
            _tokenData = new TokenData();
            _tokenData.Header.Add("typ", "JWT");
            _tokenData.Header.Add("alg", JsonWebAlgorithm.GetAlgorithm("HMACSHA256"));
            _tokenData.Payload.Add("aud", "signin.education.gov.uk");
            _tokenData.Payload.Add("iss", configuration.Value.ClientId);

            SecretKey = Encoding.UTF8.GetBytes(configuration.Value.APIServiceSecret);
        }

        public string CreateToken()
        {
            var headerBytes = Encoding.UTF8.GetBytes(_tokenDataSerializer.Serialize(_tokenData.Header));
            var payloadBytes = Encoding.UTF8.GetBytes(_tokenDataSerializer.Serialize(_tokenData.Payload));
            var bytesToSign = Encoding.UTF8.GetBytes($"{Base64Encode(headerBytes)}.{Base64Encode(payloadBytes)}");
            var signedBytes = SignToken(SecretKey, bytesToSign);

            return $"{Base64Encode(headerBytes)}.{Base64Encode(payloadBytes)}.{Base64Encode(signedBytes)}";
        }

        private byte[] SignToken(byte[] key, byte[] bytesToSign)
        {
            using (var algorithm = HMAC.Create(AuthConfig.Algorithm))
            {
                algorithm.Key = key;
                return algorithm.ComputeHash(bytesToSign);
            }
        }
        private string Base64Encode(byte[] stringInput)
        {
            return Convert.ToBase64String(stringInput).Split(new[] { '=' })[0].Replace('+', '-').Replace('/', '_');
        }
    }
}

