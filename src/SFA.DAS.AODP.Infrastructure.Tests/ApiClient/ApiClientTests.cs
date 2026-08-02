using System.Net;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Application.Commands.Rollover;
using SFA.DAS.AODP.Domain.Rollover;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.UnitTests.Helper.Testing;
using Shouldly;
using AodpApiClient = SFA.DAS.AODP.Infrastructure.ApiClient.ApiClient;

namespace SFA.DAS.AODP.Infrastructure.UnitTests.ApiClient;

public class ApiClientTests : UnitTest
{
    [Fact]
    public async Task PostWithResponseCodeAsMultipart_WhenRequestIsProvided_SendsMultipartFormData()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new AodpOuterApiSettings
        {
            BaseUrl = "https://aodp.example/",
            Key = "subscription-key"
        });
        var sut = new AodpApiClient(httpClient, options);
        var request = new GetQualificationVersionsForRolloverQueryBuilderApiRequest(
            new RolloverQueryBuilderRequest(
                LevelIds: [1],
                TypeIds: [2],
                SectorSubjectAreaIds: ["01"],
                AwardingOrganisationIds: ["AO1"]));
        KeyValuePair<string, string>[] expected =
        [
            new("LevelIds", "1"),
            new("TypeIds", "2"),
            new("SectorSubjectAreaIds", "01"),
            new("AwardingOrganisationIds", "AO1")
        ];

        // Act
        await sut.PostWithResponseCodeAsMultipart<object>(request);

        // Assert
        handler.ContentType.ShouldStartWith("multipart/form-data");
        handler.FormData.ShouldBe(expected);
        handler.RequestUri.ShouldBe(new Uri("https://aodp.example/api/rollover/querybuilder/qualificationversions"));
    }

    [Fact]
    public async Task PostWithResponseCodeAsJsonFile_WhenRequestIsProvided_SendsOneJsonFile()
    {
        // Arrange
        var handler = new RecordingHttpMessageHandler();
        var sut = new AodpApiClient(new HttpClient(handler), Options.Create(new AodpOuterApiSettings
        {
            BaseUrl = "https://aodp.example/",
            Key = "subscription-key"
        }));
        var request = new SubmitRolloverExtensionApiRequest
        {
            Data = new SubmitRolloverExtensionCommand
            {
                Items = [new FundingExtensionItem { Qan = "12345678" }]
            }
        };

        // Act
        await sut.PostWithResponseCodeAsJsonFile<object>(request);

        // Assert
        handler.ContentType.ShouldStartWith("multipart/form-data");
        handler.FormData.Length.ShouldBe(1);
        handler.FormData.Single().Key.ShouldBe("payload");
        handler.FormData.Single().Value.ShouldContain("\"Qan\":\"12345678\"");
        handler.FileNames.ShouldBe(["payload.json"]);
        handler.PartContentTypes.ShouldBe(["application/json"]);
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public string ContentType { get; private set; } = string.Empty;
        public KeyValuePair<string, string>[] FormData { get; private set; } = [];
        public Uri? RequestUri { get; private set; }
        public string[] FileNames { get; private set; } = [];
        public string[] PartContentTypes { get; private set; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            ContentType = request.Content?.Headers.ContentType?.MediaType ?? string.Empty;
            RequestUri = request.RequestUri;

            var multipartContent = request.Content.ShouldBeOfType<MultipartFormDataContent>();
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

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        }
    }
}
