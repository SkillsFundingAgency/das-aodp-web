using System.Net;
using System.Text.Json;
using AutoFixture;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace SFA.DAS.AODP.Stubs
{
    internal class Program
    {
        private static WireMockServer _fakeIdentityService;
        private const int PortIdentityService = 6001;

        static void Main(string[] args)
        {
            try
            {

                _fakeIdentityService = IdentityServerBuilder.Create(PortIdentityService)                       
                        .WithWellKnownOpenIdEndpoint()
                        .WithSigningKeyInfoEndpoint()
                        .WithAuthorizeEndpoint()
                        .WithUserInfoEndpoint()
                        .WithTokenEndpoint()                        
                        .Build();
            }
            finally
            {
                _fakeIdentityService?.Stop();
                _fakeIdentityService?.Dispose();
            }
        }        
    }
}
