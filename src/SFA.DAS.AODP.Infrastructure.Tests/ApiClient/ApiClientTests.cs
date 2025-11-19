using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using SFA.DAS.AODP.Application.Commands.Import;
using SFA.DAS.AODP.Models.Settings;
using System.Net;
using System.Text;

namespace SFA.DAS.AODP.Infrastructure.UnitTests.ApiClient;

public class ApiClientTests
{
    private const string BaseUrl = "https://api.test/";
    private const string SubscriptionKey = "test-key";

    [Fact]
    public async Task Get_ReturnsDeserialized_WhenSuccess()
    {
        // Arrange
        var expected = new SampleResponse { Message = "ok", Code = 1 };
        var expectedJson = JsonConvert.SerializeObject(expected);

        var handler = CreateHandlerMock(expectedJson);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        // Act
        var response = await client.Get<SampleResponse>(new TestGetRequest("endpoint"));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(response);
            Assert.Equal(expected.Message, response.Message);
            Assert.Equal(expected.Code, response.Code);

            Assert.NotNull(response);
            handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        });
    }

    [Fact]
    public async Task Get_Throws_WhenNotSuccess()
    {
        // Arrange
        var errorBody = "{ \"error\": \"bad request\" }";
        var handler = CreateHandlerMock(errorBody, HttpStatusCode.BadRequest);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());
        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.Get<SampleResponse>(new TestGetRequest("endpoint")));
    }

    [Fact]
    public async Task Put_Generic_ReturnsDeserialized_WhenSuccess()
    {
        // Arrange
        var expected = new SampleResponse { Message = "put-ok", Code = 2 };
        var expectedJson = JsonConvert.SerializeObject(expected);

        var handler = CreateHandlerMock(expectedJson);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        var requestData = new SampleRequest { Name = "test" };

        // Act
        var response = await client.Put<SampleResponse>(new TestPutRequest("put-endpoint", requestData));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(expected.Message, response.Message);
            Assert.Equal(expected.Code, response.Code);
            handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        });
    }

    [Fact]
    public async Task Put_Generic_Throws_WhenNotSuccess()
    {
        // Arrange
        var errorBody = "{ \"error\": \"bad request\" }";
        var handler = CreateHandlerMock(errorBody, HttpStatusCode.BadRequest);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.Put<SampleResponse>(new TestPutRequest("put-endpoint", new SampleRequest())));
    }

    [Fact]
    public async Task Put_Void_Succeeds_WhenSuccess()
    {
        // Arrange
        var expected = new SampleResponse { Message = "put-ok", Code = 2 };
        var expectedJson = JsonConvert.SerializeObject(expected);
        var handler = CreateHandlerMock(expectedJson);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        // Act
        await client.Put(new TestPutRequest("put-endpoint", new SampleRequest()));

        // Assert
        handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Put_Void_Throws_WhenNotSuccess()
    {
        // Arrange
        var errorBody = "{ \"error\": \"bad request\" }";
        var handler = CreateHandlerMock(errorBody, HttpStatusCode.BadRequest);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.Put(new TestPutRequest("put-endpoint", new SampleRequest())));
    }

    [Fact]
    public async Task PutWithResponseCode_Returns_ApiResponse_WithStatusCode()
    {
        // Arrange
        var expected = new SampleResponse { Message = "putresp", Code = 3 };
        var expectedJson = JsonConvert.SerializeObject(expected);

        var handler = CreateHandlerMock(expectedJson);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        // Act
        var response = await client.PutWithResponseCode<SampleResponse>(new TestPutRequest("put-endpoint", new SampleRequest()));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(response);
            Assert.Equal(expected.Message, response.Body.Message);
            Assert.Equal(expected.Code, response.Body.Code);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        });
    }

    [Fact]
    public async Task PostWithResponseCode_Generic_Succeeds()
    {
        // Arrange
        var expected = new SampleResponse { Message = "post-ok", Code = 4 };
        var expectedJson = JsonConvert.SerializeObject(expected);

        var handler = CreateHandlerMock(expectedJson);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        // Act
        var result = await client.PostWithResponseCode<SampleResponse>(new TestPostRequest("post-endpoint", new SampleRequest()));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(result);
            Assert.Equal(expected.Message, result.Message);
            handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        });
    }

    [Fact]
    public async Task PostWithResponseCode_Generic_Throws_WhenNotSuccess()
    {
        // Arrange
        var errorBody = "{ \"error\": \"bad request\" }";
        var handler = CreateHandlerMock(errorBody, HttpStatusCode.BadRequest);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.PostWithResponseCode<SampleResponse>(new TestPostRequest("post-endpoint", new SampleRequest())));
    }

    [Fact]
    public async Task PostWithResponseCode_Void_Succeeds()
    {
        // Arrange
        var expected = new SampleResponse { Message = "put-ok", Code = 2 };
        var expectedJson = JsonConvert.SerializeObject(expected);
        var handler = CreateHandlerMock(expectedJson);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        await client.PostWithResponseCode(new TestPostRequest("post-endpoint", new SampleRequest()));

        // Assert
        handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Delete_Succeeds()
    {
        // Arrange
        var expected = new SampleResponse { Message = "put-ok", Code = 2 };
        var expectedJson = JsonConvert.SerializeObject(expected);
        var handler = CreateHandlerMock(expectedJson);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        // Act
        await client.Delete(new TestDeleteRequest("delete-endpoint"));

        // Assert
        handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task PostWithMultipartFormData_ReturnsDeserialized_AndHasMultipartContent()
    {
        var expected = new ImportDefundingListResponse { Message = "file-ok", ImportedCount = 5 };
        var expectedJson = JsonConvert.SerializeObject(expected);

        // Prepare a fake IFormFile
        var fileContentBytes = Encoding.UTF8.GetBytes("file-content");
        var ms = new MemoryStream(fileContentBytes);
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.FileName).Returns("test.txt");
        formFileMock.Setup(f => f.ContentType).Returns("text/plain");
        formFileMock.Setup(f => f.Length).Returns(ms.Length);
        formFileMock.Setup(f => f.OpenReadStream()).Returns(() =>
        {
            return new MemoryStream(fileContentBytes);
        });

        var handler = CreateHandlerMock(expectedJson);

        var client = new SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient(CreateHttpClient(handler), CreateOptions());

        var postRequest = new TestPostRequest("upload-endpoint", formFileMock.Object);

        // Act
        var result = await client.PostWithMultipartFormData<ImportDefundingListResponse>(postRequest);


        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(result);
            Assert.Equal(result.Message, expected.Message);
            Assert.Equal(result.ImportedCount, expected.ImportedCount);
            handler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        });
    }

    private HttpClient CreateHttpClient(Mock<HttpMessageHandler> handlerMock)
    {
        var client = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(BaseUrl)
        };
        return client;
    }

    private IOptions<AodpOuterApiSettings> CreateOptions()
    {
        return Options.Create(new AodpOuterApiSettings
        {
            BaseUrl = BaseUrl,
            Key = SubscriptionKey
        });
    }

    private Mock<HttpMessageHandler> CreateHandlerMock(string expectedJson, HttpStatusCode code = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
               "SendAsync",
               ItExpr.IsAny<HttpRequestMessage>(),
               ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
           {
               var response = new HttpResponseMessage(code)
               {
                   Content = new StringContent(expectedJson, Encoding.UTF8, "application/json")
               };
               return response;
           })
           .Verifiable();

        return handlerMock;
    }

    // Simple DTOs for tests
    private class SampleResponse
    {
        public string Message { get; set; } = string.Empty;
        public int Code { get; set; }
    }

    private class SampleRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    // Implement request interfaces for tests
    private class TestGetRequest : SFA.DAS.AODP.Domain.Interfaces.IGetApiRequest
    {
        public string GetUrl { get; }
        public TestGetRequest(string url) => GetUrl = url;
    }

    private class TestPutRequest : SFA.DAS.AODP.Domain.Interfaces.IPutApiRequest
    {
        public string PutUrl { get; }
        public object Data { get; set; }
        public TestPutRequest(string url, object data) { PutUrl = url; Data = data; }
    }

    private class TestPostRequest : SFA.DAS.AODP.Domain.Interfaces.IPostApiRequest
    {
        public string PostUrl { get; }
        public object Data { get; set; }
        public TestPostRequest(string url, object data) { PostUrl = url; Data = data; }
    }

    private class TestDeleteRequest : SFA.DAS.AODP.Domain.Interfaces.IDeleteApiRequest
    {
        public string DeleteUrl { get; }
        public TestDeleteRequest(string url) => DeleteUrl = url;
    }
}
