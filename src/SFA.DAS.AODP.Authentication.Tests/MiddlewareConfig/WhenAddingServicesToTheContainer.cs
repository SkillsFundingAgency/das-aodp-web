//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.AspNetCore.Authentication;
//using SFA.DAS.AODP.Authentication.Configuration;
//using SFA.DAS.AODP.Authentication.Services;
//using SFA.DAS.AODP.Authentication.DfeSignInApi.JWTHelpers;
//using SFA.DAS.AODP.Authentication.DfeSignInApi.Client;
//using SFA.DAS.AODP.Authentication.Extensions;

//namespace SFA.DAS.DfESignIn.Auth.UnitTests.AppStart;


//public class WhenAddingServicesToTheContainer
//{



//    [Theory]
//    [InlineData(typeof(DfEOidcConfiguration))]
//    [InlineData(typeof(IDfESignInService))]
//    [InlineData(typeof(ITokenDataSerializer))]
//    [InlineData(typeof(ITokenBuilder))]
//    [InlineData(typeof(IDFESignInAPIClient))]
//    [InlineData(typeof(ITicketStore))]
//    public void Then_The_Dependencies_Are_Correctly_Resolved(Type toResolve)
//    {
//        var serviceCollection = new ServiceCollection();
//        SetupServiceCollection(serviceCollection);

//        var provider = serviceCollection.BuildServiceProvider();

//        var type = provider.GetService(toResolve);

//        Assert.NotNull(type);
//    }

//    [Test]
//    public void Then_DfELoginSessionConnectionString_IsNull_AddDistributedMemoryCache_RegistersDistributedMemoryCache()
//    {
//        // Arrange
//        var configuration = GenerateConfiguration();
//        configuration["DfEOidcConfiguration:DfELoginSessionConnectionString"] = string.Empty;

//        var serviceCollection = new ServiceCollection();
//        serviceCollection.AddServiceRegistration(configuration, typeof(TestCustomServiceRole), ClientName.ProviderRoatp);

//        // Assert
//        var serviceProvider = serviceCollection.BuildServiceProvider();
//        var distributedCache = serviceProvider.GetService<IDistributedCache>();
//        Assert.That(distributedCache, Is.Not.Null);
//        Assert.That(distributedCache, Is.InstanceOf<MemoryDistributedCache>());
//    }

//    //[Test]
//    //public void Then_DfELoginSessionConnectionString_IsNotNull_AddStackExchangeRedisCache_RegistersDistributedMemoryCache()
//    //{
//    //    // Arrange
//    //    var configuration = GenerateConfiguration();

//    //    var serviceCollection = new ServiceCollection();
//    //    serviceCollection.add
//    //    serviceCollection.AddServiceRegistration(configuration, typeof(TestCustomServiceRole), ClientName.ProviderRoatp);

//    //    // Assert
//    //    var serviceProvider = serviceCollection.BuildServiceProvider();
//    //    var distributedCache = serviceProvider.GetService<IDistributedCache>();
//    //    Assert.That(distributedCache, Is.Not.Null);
//    //    Assert.That(distributedCache, Is.InstanceOf<RedisCache>());
//    //}

//    [Test]
//    public async Task Then_ConfigureDfESignInAuthentication_Should_Have_Expected_AuthenticationCookie()
//    {
//        // Arrange
//        var configuration = GenerateConfiguration();
//        var serviceCollection = new ServiceCollection();
//        serviceCollection.AddAndConfigureDfESignInAuthentication(configuration, $"{typeof(AddServiceRegistrationExtension).Assembly.GetName().Name}.Auth", typeof(TestCustomServiceRole), ClientName.ProviderRoatp);
//        var expectedAuthSchemeNames = new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme };

//        // Assert
//        var serviceProvider = serviceCollection.BuildServiceProvider();
//        var authenticationSchemeProvider = serviceProvider.GetService<IAuthenticationSchemeProvider>();
//        Assert.That(authenticationSchemeProvider, Is.Not.Null);
        
//        var authenticationSchemes = await authenticationSchemeProvider?.GetAllSchemesAsync()!;
//        var authSchemeList = authenticationSchemes?.ToList();
//        var actualAuthSchemeNames = authSchemeList?.Select(args => args.Name).ToArray();
//        Assert.Multiple(() =>
//        {
//            Assert.NotNull(authSchemeList);
//            bool isSuperset = IsSuperset(actualAuthSchemeNames, expectedAuthSchemeNames);

//            // Assert
//            Assert.True(isSuperset);
//        });


//        static bool IsSuperset<T>(T[] superset, T[] subset)
//        {
//            HashSet<T> supersetSet = new HashSet<T>(superset);
//            return subset.All(item => supersetSet.Contains(item));
//        }

//    private static void SetupServiceCollection(IServiceCollection serviceCollection)
//    {
//        var configuration = GenerateConfiguration();
//        serviceCollection.BuildServiceProvider().AddServiceRegistration(configuration, typeof(TestCustomServiceRole), ClientName.ProviderRoatp);
//    }

//    //private static IConfigurationRoot GenerateConfiguration()
//    //{
//    //    var configSource = new MemoryConfigurationSource
//    //    {
//    //        InitialData = new List<KeyValuePair<string, string>>
//    //        {
//    //            new("DfEOidcConfiguration:BaseUrl", "https://test.com/"),
//    //            new("DfEOidcConfiguration:ClientId", "1234567"),
//    //            new("DfEOidcConfiguration:APIServiceSecret", "1234567"),
//    //            new("DfEOidcConfiguration:KeyVaultIdentifier", "https://test.com/"),
//    //            new("ProviderSharedUIConfiguration:DashboardUrl", "https://test.com/"),
//    //            new("DfEOidcConfiguration:DfELoginSessionConnectionString", "https://test.com/"),
//    //            new("DfEOidcConfiguration:LoginSlidingExpiryTimeOutInMinutes", "30"),
//    //            new("ResourceEnvironmentName", "test"),
//    //        }
//    //    };

//    //    var provider = new MemoryConfigurationProvider(configSource);

//    //    return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
//    //}

//    //public class TestCustomClaims : ICustomClaims
//    //{
//    //    public IEnumerable<Claim?> GetClaims(TokenValidatedContext tokenValidatedContext)
//    //    {
//    //        throw new NotImplementedException();
//    //    }
//    //}

//    //public class TestCustomServiceRole : ICustomServiceRole
//    //{
//    //    public string RoleClaimType => throw new NotImplementedException();
//    //    public CustomServiceRoleValueType RoleValueType => CustomServiceRoleValueType.Name;
//    //}
//}