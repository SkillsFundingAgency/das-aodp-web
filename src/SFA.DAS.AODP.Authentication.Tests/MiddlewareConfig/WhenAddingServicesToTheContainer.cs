using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using SFA.DAS.AODP.Authentication.Configuration;
using SFA.DAS.AODP.Authentication.Services;
using SFA.DAS.AODP.Authentication.DfeSignInApi.JWTHelpers;
using SFA.DAS.AODP.Authentication.Interfaces;
using SFA.DAS.AODP.Authentication.Enums;
using SFA.DAS.AODP.Authentication.Extensions;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Client;

namespace SFA.DAS.DfESignIn.Auth.UnitTests.AppStart;


public class WhenAddingServicesToTheContainer
{
    [Theory]
    [InlineData(typeof(DfEOidcConfiguration))]
    [InlineData(typeof(IDfESignInService))]
    [InlineData(typeof(ITokenDataSerializer))]
    [InlineData(typeof(ITokenBuilder))]
    [InlineData(typeof(IDFESignInAPIClient))]
    [InlineData(typeof(ITicketStore))]
    public void Then_The_Dependencies_Are_Correctly_Resolved(Type toResolve)
    {
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        serviceCollection.AddDistributedMemoryCache();
        var provider = serviceCollection.BuildServiceProvider();

        var type = provider.GetService(toResolve);

        Assert.NotNull(type);
    }

    [Fact]
    public async Task Then_ConfigureDfESignInAuthentication_Should_Have_Expected_AuthenticationCookie()
    {
        // Arrange
        var configuration = GenerateConfiguration();
        var serviceCollection = new ServiceCollection();
        SetupServiceCollection(serviceCollection);
        var expectedAuthSchemeNames = new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme };

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var authenticationSchemeProvider = serviceProvider.GetService<IAuthenticationSchemeProvider>();
        Assert.NotNull(authenticationSchemeProvider);

        var authenticationSchemes = await authenticationSchemeProvider?.GetAllSchemesAsync()!;
        var authSchemeList = authenticationSchemes?.ToList();
        var actualAuthSchemeNames = authSchemeList?.Select(args => args.Name);

        var hasExpected = new HashSet<string>(expectedAuthSchemeNames);
        var hasActual = new HashSet<string>(actualAuthSchemeNames);

        var isSuperSet = hasExpected.IsSupersetOf(expectedAuthSchemeNames);
        Assert.Multiple(() =>
        {
            Assert.NotNull(authSchemeList);
            Assert.True(isSuperSet);
        });
    }

    private static void SetupServiceCollection(IServiceCollection serviceCollection)
    {


        serviceCollection.AddTransient(typeof(ICustomServiceRole), typeof(CustomServiceRole));
        var configuration = GenerateConfiguration();
        serviceCollection.AddDfeSignIn(configuration, "", typeof(CustomServiceRole));
    }

    private static IConfigurationRoot GenerateConfiguration()
    {
        var configSource = new MemoryConfigurationSource
        {
            InitialData = new List<KeyValuePair<string, string>>
            {
                new("DfEOidcConfiguration:BaseUrl", "https://test.com/"),
                new("DfEOidcConfiguration:ClientId", "1234567"),
                new("DfEOidcConfiguration:APIServiceSecret", "1234567"),
                new("DfEOidcConfiguration:KeyVaultIdentifier", "https://test.com/"),
                new("ProviderSharedUIConfiguration:DashboardUrl", "https://test.com/"),
                new("DfEOidcConfiguration:DfELoginSessionConnectionString", "https://test.com/"),
                new("DfEOidcConfiguration:LoginSlidingExpiryTimeOutInMinutes", "30")
            }
        };

        var provider = new MemoryConfigurationProvider(configSource);

        return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
    }

    public class CustomServiceRole : ICustomServiceRole
    {
        public string RoleClaimType => "http://schemas.portal.com/service";
        public CustomServiceRoleValueType RoleValueType => CustomServiceRoleValueType.Code;
    }
}