﻿using System.Net;
using System.Security.Claims;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Client;
using SFA.DAS.AODP.Authentication.DfeSignInApi.JWTHelpers;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Models;
using SFA.DAS.AODP.Authentication.DfeSignInApi.Models.ApiResponses;

namespace SFA.DAS.AODP.Authentication.Tests.Api.Client
{
    public class DfeSignInApiTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<ITokenBuilder> _tokenBuilder;

        public DfeSignInApiTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _tokenBuilder = new Mock<ITokenBuilder>();
        }

        [Fact]
        public async Task Get_Return_UserAccessResponse_When_HttpResponse_IsValid()
        {
            // ARRANGE
            var userId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var orgId = Guid.NewGuid();
            var authToken = Guid.NewGuid();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent($"{{'userId':'{userId}','serviceId':'{serviceId}', 'organisationId':'{orgId}'}}"),
                })
                .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var subjectUnderTest = new DFESignInAPIClient(httpClient, _tokenBuilder.Object);
            _tokenBuilder.Setup(x => x.CreateToken()).Returns(authToken.ToString());

            // ACT
            var result = await subjectUnderTest.Get<UserAccessResponse>("api/test/whatever");

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(serviceId, result.ServiceId);
            Assert.Equal(orgId, result.OrganisationId);

            // also check the 'http' call was like we expected it
            var expectedUri = new Uri("http://test.com/api/test/whatever");

            // verify if called at least once.
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1), // we expected a single external request
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get  // we expected a GET request
                        && req.RequestUri == expectedUri
                    && req.Headers.Authorization.Scheme == "Bearer"
                    && req.Headers.Authorization.Parameter == authToken.ToString()
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }


        [Fact]
        public async Task Get_Return_UserAccessResponse_Default_When_HttpResponse_IsInValid()
        {
            // ARRANGE
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = null,
                })
                .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var subjectUnderTest = new DFESignInAPIClient(httpClient, _tokenBuilder.Object);

            // ACT
            var result = await subjectUnderTest.Get<UserAccessResponse>("api/test/whatever");

            // ASSERT
            Assert.Null(result);

            // also check the 'http' call was like we expected it
            var expectedUri = new Uri("http://test.com/api/test/whatever");

            // verify if called at least once.
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1), // we expected a single external request
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get  // we expected a GET request
                        && req.RequestUri == expectedUri // to this uri
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }


        [Fact]
        public async Task Get_Return_UserOrganistionResponse_When_HttpResponse_IsValid()
        {
            // ARRANGE
            var orgId = Guid.NewGuid();
            var orgName = "test1";
            var authToken = Guid.NewGuid();

            var fixter = new Fixture();

            var userOrganisationResponses = fixter.CreateMany<UserOrganisationResponse>(10).ToList();

            var randomIndex = new Random().Next(1, 10);
            userOrganisationResponses[randomIndex].Id = orgId.ToString();
            userOrganisationResponses[randomIndex].Name = orgName;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(userOrganisationResponses))
                })
                .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var subjectUnderTest = new DFESignInAPIClient(httpClient, _tokenBuilder.Object);
            _tokenBuilder.Setup(x => x.CreateToken()).Returns(authToken.ToString());

            // ACT
            var result = await subjectUnderTest.Get<List<UserOrganisationResponse>>("api/test/whatever");
            // ASSERT
            Assert.NotNull(result);
            Assert.Contains(result, item => item.Id == orgId.ToString());
            Assert.Contains(result, item => item.Name == orgName);

            // also check the 'http' call was like we expected it
            var expectedUri = new Uri("http://test.com/api/test/whatever");

            // verify if called at least once.
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1), // we expected a single external request
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get  // we expected a GET request
                        && req.RequestUri == expectedUri
                    && req.Headers.Authorization.Scheme == "Bearer"
                    && req.Headers.Authorization.Parameter == authToken.ToString()
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }


        [Fact]
        public async Task Get_Return_UserOrganisationResponse_Default_When_HttpResponse_IsInValid()
        {
            // ARRANGE
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = null,
                })
                .Verifiable();

            // use real http client with mocked handler here
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://test.com/"),
            };

            var subjectUnderTest = new DFESignInAPIClient(httpClient, _tokenBuilder.Object);

            // ACT
            var result = await subjectUnderTest.Get<List<UserOrganisationResponse>>("api/test/whatever");

            // ASSERT
            Assert.Null(result);

            // also check the 'http' call was like we expected it
            var expectedUri = new Uri("http://test.com/api/test/whatever");

            // verify if called at least once.
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1), // we expected a single external request
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get  // we expected a GET request
                        && req.RequestUri == expectedUri // to this uri
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

    }
}
