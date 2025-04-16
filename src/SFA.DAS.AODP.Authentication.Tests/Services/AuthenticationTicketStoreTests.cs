using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using SFA.DAS.AODP.Authentication.Configuration;
using SFA.DAS.AODP.Authentication.Services;
using AutoFixture;

namespace SFA.DAS.DfESignIn.Auth.UnitTests.Services
{
    public class AuthenticationTicketStoreTests
    {
        [Fact]
        public async Task Then_The_Ticket_Is_Added_To_The_Store()
        {
            var fixture = new Fixture();
            var ticket = fixture.Create<AuthenticationTicket>();
            DfEOidcConfiguration config = new() { ClientId = "123" };
            int expiryTime = 1;
            Mock<IDistributedCache> distributedCache = new();
            Mock<IOptions<DfEOidcConfiguration>> configuration = new();
            configuration.Setup(c => c.Value).Returns(config);
            AuthenticationTicketStore authenticationTicketStore = new(distributedCache.Object, configuration.Object);
            configuration.Object.Value.LoginSlidingExpiryTimeOutInMinutes = expiryTime;

            var result = await authenticationTicketStore.StoreAsync(ticket);

            Assert.True(Guid.TryParse(result, out var actualKey));
            distributedCache.Verify(x => x.SetAsync(
                actualKey.ToString(),
                It.Is<byte[]>(c => TicketSerializer.Default.Deserialize(c)!.AuthenticationScheme == ticket.AuthenticationScheme),
                It.Is<DistributedCacheEntryOptions>(c
                    => c.SlidingExpiration != null && c.SlidingExpiration.Value.Minutes == TimeSpan.FromMinutes(expiryTime).Minutes),
                It.IsAny<CancellationToken>()
                ), Times.Once);
        }

        [Fact]
        public async Task Then_The_Ticket_Is_Retrieved_By_Id_From_The_Store()

        {
            var fixture = new Fixture();
            var ticket = fixture.Create<AuthenticationTicket>();

            string key = "1";
            Mock<IOptions<DfEOidcConfiguration>> config = new();
            Mock<IDistributedCache> distributedCache = new();
            AuthenticationTicketStore authenticationTicketStore = new(distributedCache.Object, config.Object);


            distributedCache.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(TicketSerializer.Default.Serialize(ticket));

            var result = await authenticationTicketStore.RetrieveAsync(key);

            Assert.Equivalent(result, ticket);
        }

        [Fact]
        public async Task Then_Null_Is_Returned_If_The_Key_Does_Not_Exist()
        {
            string key = "1";

            Mock<IDistributedCache> distributedCache = new();
            Mock<IOptions<DfEOidcConfiguration>> config = new();
            AuthenticationTicketStore authenticationTicketStore = new(distributedCache.Object, config.Object);
            distributedCache.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null!);

            var result = await authenticationTicketStore.RetrieveAsync(key);

            Assert.Null(result);
        }

        [Fact]
        public async Task Then_The_Key_Is_Refreshed()
        {

            var fixture = new Fixture();
            var ticket = fixture.Create<AuthenticationTicket>();
            string key = "1";
            Mock<IDistributedCache> distributedCache = new();
            Mock<IOptions<DfEOidcConfiguration>> config = new();

            AuthenticationTicketStore authenticationTicketStore = new(distributedCache.Object, config.Object);


            await authenticationTicketStore.RenewAsync(key, ticket);

            distributedCache.Verify(x => x.RefreshAsync(key, CancellationToken.None));
        }

        [Fact]
        public async Task Then_The_Key_Is_Removed()
        {
            var fixture = new Fixture();
            var ticket = fixture.Create<AuthenticationTicket>();
            string key = "1";
            Mock<IDistributedCache> distributedCache = new();
            Mock<IOptions<DfEOidcConfiguration>> config = new();

            AuthenticationTicketStore authenticationTicketStore = new(distributedCache.Object, config.Object);



            await authenticationTicketStore.RemoveAsync(key);

            distributedCache.Verify(x => x.RemoveAsync(key, CancellationToken.None));
        }
    }
}
