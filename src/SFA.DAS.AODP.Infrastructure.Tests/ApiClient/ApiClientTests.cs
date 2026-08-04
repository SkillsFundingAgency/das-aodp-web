using System.Net;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Domain.Interfaces;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.UnitTests.Helper.Testing;
using Shouldly;
using AodpApiClient = SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient;

namespace SFA.DAS.AODP.Infrastructure.UnitTests.ApiClient;

public class ApiClientTests : UnitTest
{
    [Fact]
    public async Task Get_WhenResponseIsSuccessful_SendsAuthenticatedRequestAndReturnsBody()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var sut = CreateSut(handler);
        var request = new TestGetRequest("api/qualifications/123");

        // Act
        var result = await sut.Get<TestResponse>(request);

        // Assert
        result.ResultMessage.ShouldBe("Applied");
        handler.Method.ShouldBe(HttpMethod.Get);
        handler.RequestUri.ShouldBe(new Uri("https://aodp.example/api/qualifications/123"));
        handler.Headers["Ocp-Apim-Subscription-Key"].ShouldBe("subscription-key");
        handler.Headers["X-Version"].ShouldBe("1");
    }

    [Fact]
    public async Task Get_WhenResponseIsNotSuccessful_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.BadRequest);
        var sut = CreateSut(handler);

        // Act
        var action = () => sut.Get<TestResponse>(new TestGetRequest("api/qualifications/123"));

        // Assert
        await Should.ThrowAsync<HttpRequestException>(action);
    }

    [Fact]
    public async Task Put_WhenResponseIsSuccessful_SendsJsonAndReturnsBody()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var sut = CreateSut(handler);
        var request = new TestPutRequest("api/qualifications/123", new { Status = "Approved" });

        // Act
        var result = await sut.Put<TestResponse>(request);

        // Assert
        result.ResultMessage.ShouldBe("Applied");
        handler.Method.ShouldBe(HttpMethod.Put);
        handler.Body.ShouldBe("{\"Status\":\"Approved\"}");
        handler.ContentType.ShouldBe("application/json; charset=utf-8");
        handler.VersionPolicy.ShouldBe(HttpVersionPolicy.RequestVersionOrLower);
    }

    [Fact]
    public async Task Put_WhenResponseIsNotSuccessful_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.UnprocessableEntity);
        var sut = CreateSut(handler);
        var request = new TestPutRequest("api/qualifications/123", new { Status = "Invalid" });

        // Act
        var action = () => sut.Put<TestResponse>(request);

        // Assert
        await Should.ThrowAsync<HttpRequestException>(action);
    }

    [Fact]
    public async Task Put_WhenNoResponseIsRequired_SendsJsonRequest()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var sut = CreateSut(handler);
        var request = new TestPutRequest("api/qualifications/123", new { Status = "Approved" });

        // Act
        await sut.Put(request);

        // Assert
        handler.Method.ShouldBe(HttpMethod.Put);
        handler.Body.ShouldBe("{\"Status\":\"Approved\"}");
        handler.RequestUri.ShouldBe(new Uri("https://aodp.example/api/qualifications/123"));
    }

    [Fact]
    public async Task PutWithResponseCode_WhenResponseIsNotSuccessful_ReturnsStatusAndBody()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.Conflict);
        var sut = CreateSut(handler);
        var request = new TestPutRequest("api/qualifications/123", new { Status = "Approved" });

        // Act
        var result = await sut.PutWithResponseCode<TestResponse>(request);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        result.Body.ResultMessage.ShouldBe("Applied");
        result.ErrorContent.ShouldBeNull();
        handler.Method.ShouldBe(HttpMethod.Put);
    }

    [Fact]
    public async Task PostWithResponseCode_WhenResponseIsSuccessful_SendsJsonAndReturnsBody()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var sut = CreateSut(handler);
        var request = new TestPostRequest("api/qualifications", new { Qan = "12345678" });

        // Act
        var result = await sut.PostWithResponseCode<TestResponse>(request);

        // Assert
        result.ShouldNotBeNull();
        result.ResultMessage.ShouldBe("Applied");
        handler.Method.ShouldBe(HttpMethod.Post);
        handler.Body.ShouldBe("{\"Qan\":\"12345678\"}");
        handler.ContentType.ShouldBe("application/json; charset=utf-8");
    }

    [Fact]
    public async Task PostWithResponseCode_WhenResponseIsNotSuccessful_ThrowsHttpRequestException()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(HttpStatusCode.InternalServerError);
        var sut = CreateSut(handler);

        // Act
        var action = () => sut.PostWithResponseCode<TestResponse>(
            new TestPostRequest("api/qualifications", new { Qan = "12345678" }));

        // Assert
        await Should.ThrowAsync<HttpRequestException>(action);
    }

    [Fact]
    public async Task PostWithResponseCode_WhenResponseBodyIsNull_ReturnsNull()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(responseContent: "null");
        var sut = CreateSut(handler);
        var request = new TestPostRequest("api/qualifications", new { Qan = "12345678" });

        // Act
        var result = await sut.PostWithResponseCode<TestResponse>(request);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task PostWithResponseCodeAsMultipart_WhenRequestIsProvided_SendsFormFieldsAndReturnsBody()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var sut = CreateSut(handler);
        var request = new TestMultipartRequest(
            "api/rollover/validate",
            [
                new KeyValuePair<string, string>("Items[0].Qan", "12345678"),
                new KeyValuePair<string, string>("Items[0].FundingStreamName", "FS1")
            ]);

        // Act
        var result = await sut.PostWithResponseCodeAsMultipart<TestResponse>(request);

        // Assert
        result.ShouldNotBeNull();
        result.ResultMessage.ShouldBe("Applied");
        handler.Method.ShouldBe(HttpMethod.Post);
        handler.ContentType.ShouldStartWith("multipart/form-data");
        handler.FormData.ShouldBe(
        [
            new KeyValuePair<string, string>("Items[0].Qan", "12345678"),
            new KeyValuePair<string, string>("Items[0].FundingStreamName", "FS1")
        ]);
    }

    [Fact]
    public async Task PostWithResponseCodeAsMultipart_WhenResponseBodyIsNull_ReturnsNull()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(responseContent: "null");
        var sut = CreateSut(handler);
        var request = new TestMultipartRequest("api/rollover/validate", []);

        // Act
        var result = await sut.PostWithResponseCodeAsMultipart<TestResponse>(request);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task PostWithResponseCodeAsJsonFile_WhenQueryBuilderRequestIsProvided_SendsOneJsonFile()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var sut = CreateSut(handler);
        var request = new GetQualificationVersionsForRolloverQueryBuilderApiRequest(
            new RolloverQueryBuilderRequest(
                LevelIds: [1],
                TypeIds: [2],
                SectorSubjectAreaIds: ["01"],
                AwardingOrganisationIds: ["AO1"]));

        // Act
        var result = await sut.PostWithResponseCodeAsJsonFile<TestResponse>(request);

        // Assert
        result.ShouldNotBeNull();
        result.ResultMessage.ShouldBe("Applied");
        handler.ContentType.ShouldStartWith("multipart/form-data");
        handler.FormData.Length.ShouldBe(1);
        handler.FormData.Single().Key.ShouldBe("payload");
        handler.FormData.Single().Value.ShouldContain("\"LevelIds\":[1]");
        handler.FileNames.ShouldBe(["payload.json"]);
        handler.PartContentTypes.ShouldBe(["application/json"]);
        handler.ContentTypeHeader.ShouldNotContain("\"");
        handler.RequestUri.ShouldBe(new Uri("https://aodp.example/api/rollover/querybuilder/qualificationversions"));
    }

    [Fact]
    public async Task PostWithResponseCodeAsJsonFile_WhenResponseBodyIsNull_ReturnsNull()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler(responseContent: "null");
        var sut = CreateSut(handler);
        var request = new SubmitRolloverExtensionApiRequest
        {
            Data = new SubmitRolloverExtensionCommand
            {
                Items = [new FundingExtensionItem { Qan = "12345678" }]
            }
        };

        // Act
        var result = await sut.PostWithResponseCodeAsJsonFile<TestResponse>(request);

        // Assert
        result.ShouldBeNull();
        handler.FormData.Length.ShouldBe(1);
        handler.FormData.Single().Key.ShouldBe("payload");
        handler.FormData.Single().Value.ShouldContain("\"Qan\":\"12345678\"");
        handler.FileNames.ShouldBe(["payload.json"]);
        handler.PartContentTypes.ShouldBe(["application/json"]);
    }

    [Fact]
    public async Task PostWithResponseCode_WhenNoResponseIsRequired_SendsJsonRequest()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var sut = CreateSut(handler);
        var request = new TestPostRequest("api/qualifications", new { Qan = "12345678" });

        // Act
        await sut.PostWithResponseCode(request);

        // Assert
        handler.Method.ShouldBe(HttpMethod.Post);
        handler.Body.ShouldBe("{\"Qan\":\"12345678\"}");
        handler.RequestUri.ShouldBe(new Uri("https://aodp.example/api/qualifications"));
    }

    [Fact]
    public async Task Delete_WhenRequestIsProvided_SendsAuthenticatedDeleteRequest()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var sut = CreateSut(handler);

        // Act
        await sut.Delete(new TestDeleteRequest("api/qualifications/123"));

        // Assert
        handler.Method.ShouldBe(HttpMethod.Delete);
        handler.RequestUri.ShouldBe(new Uri("https://aodp.example/api/qualifications/123"));
        handler.Headers["Ocp-Apim-Subscription-Key"].ShouldBe("subscription-key");
    }

    private static AodpApiClient CreateSut(RecordingHttpMessageHandler handler)
    {
        return new AodpApiClient(new HttpClient(handler), Options.Create(new AodpOuterApiSettings
        {
            BaseUrl = "https://aodp.example/",
            Key = "subscription-key"
        }));
    }

    private sealed record TestGetRequest(string GetUrl) : IGetApiRequest;

    private sealed record TestDeleteRequest(string DeleteUrl) : IDeleteApiRequest;

    private sealed record TestMultipartRequest(
        string PostUrl,
        IEnumerable<KeyValuePair<string, string>> FormData) : IPostMultipartFormDataApiRequest;

    private sealed class TestPutRequest(string putUrl, object data) : IPutApiRequest
    {
        public string PutUrl { get; } = putUrl;
        public object Data { get; set; } = data;
    }

    private sealed class TestPostRequest(string postUrl, object data) : IPostApiRequest
    {
        public string PostUrl { get; } = postUrl;
        public object Data { get; set; } = data;
    }

    private sealed class TestResponse
    {
        public string ResultMessage { get; set; } = string.Empty;
    }

    private sealed class RecordingHttpMessageHandler(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string responseContent = "{\"resultMessage\":\"Applied\"}") : HttpMessageHandler
    {
        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string ContentType { get; private set; } = string.Empty;
        public string ContentTypeHeader { get; private set; } = string.Empty;
        public string Body { get; private set; } = string.Empty;
        public HttpVersionPolicy VersionPolicy { get; private set; }
        public Dictionary<string, string> Headers { get; } = [];
        public KeyValuePair<string, string>[] FormData { get; private set; } = [];
        public string[] FileNames { get; private set; } = [];
        public string[] PartContentTypes { get; private set; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            VersionPolicy = request.VersionPolicy;
            ContentType = request.Content?.Headers.ContentType?.ToString() ?? string.Empty;
            ContentTypeHeader = ContentType;
            foreach (var header in request.Headers)
            {
                Headers[header.Key] = header.Value.Single();
            }

            if (request.Content is MultipartFormDataContent multipartContent)
            {
                var formData = new List<KeyValuePair<string, string>>();
                var fileNames = new List<string>();
                var partContentTypes = new List<string>();
                foreach (var content in multipartContent)
                {
                    var name = content.Headers.ContentDisposition?.Name?.Trim('"') ?? string.Empty;
                    var value = await content.ReadAsStringAsync(cancellationToken);
                    formData.Add(new KeyValuePair<string, string>(name, value));
                    fileNames.Add(content.Headers.ContentDisposition?.FileName?.Trim('"') ?? string.Empty);
                    partContentTypes.Add(content.Headers.ContentType?.MediaType ?? string.Empty);
                }

                FormData = formData.ToArray();
                FileNames = fileNames.ToArray();
                PartContentTypes = partContentTypes.ToArray();
            }
            else if (request.Content is not null)
            {
                Body = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseContent)
            };
        }
    }
}
