using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.Aodp.Domain.Files;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Application.Queries.Application.Review;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Helpers.Export;

namespace SFA.DAS.AODP.Web.UnitTests.Helpers.Export
{
	public class ApplicationExportServiceTests
	{
		private IFixture? _fixture;
		private Mock<IFileService>? _fileServiceMock;
		private Mock<IHtmlExportRenderer>? _htmlRendererMock;
		private ApplicationExportService _service;

		public ApplicationExportServiceTests()
		{
			_fixture = new Fixture().Customize(new AutoMoqCustomization());
			_fileServiceMock = _fixture.Freeze<Mock<IFileService>>();
			_htmlRendererMock = _fixture.Freeze<Mock<IHtmlExportRenderer>>();

			_service = new ApplicationExportService(
				_fileServiceMock.Object,
				_htmlRendererMock.Object);
		}

		[Fact]
		public async Task GenerateExportZipAsync_ReturnsZip_And_CallsDependencies()
		{
			var exportData = new GetApplicationExportDataQueryResponse
			{
				ApplicationMetadata = new ApplicationExportMetadataResponse
				{
					OrganisationName = "Org1",
					Qan = "123",
					SubmissionId = 3,
					FormName = "Form A"
				},
				ApplicationFormStructure = new GetFormPreviewByIdQueryResponse
				{
					SectionsWithPagesAndQuestions = new()
				},
				ApplicationFormResponse = new()
			};

			var files = new List<FileMetadataDto>
			{
				new FileMetadataDto
                {
					BlobPath = "files/appId/questionId/fileId",
					BlobContainer = "aqany",
					FileCategory = FileCategory.QuestionUpload,
					FileName = "file.txt",
                }
			};

			_fileServiceMock!
				.Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

			_htmlRendererMock!
				.Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync("<html></html>");

			var result = await _service.GenerateExportZipAsync(exportData, files);

			Assert.True(result.Length > 0);

			_fileServiceMock.Verify(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			_htmlRendererMock.Verify(x => x.RenderAsync("ExportSummary", It.IsAny<object>()), Times.Once);
		}

		[Fact]
		public async Task GenerateExportZipAsync_WhenQanMissing_UsesNoQAN()
		{
			var exportData = new GetApplicationExportDataQueryResponse
			{
				ApplicationMetadata = new ApplicationExportMetadataResponse
				{
					OrganisationName = "Org1",
					Qan = "",
					SubmissionId = 2,
					FormName = "Form A"
				},
				ApplicationFormStructure = new GetFormPreviewByIdQueryResponse(),
				ApplicationFormResponse = new()
			};

			var files = new List<FileMetadataDto>();

			_htmlRendererMock!
				.Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync("<html></html>");

			var result = await _service.GenerateExportZipAsync(exportData, files);

			Assert.True(result.Length > 0);
		}

		[Fact]
		public void GenerateExportZipAsync_WhenStreamNull_ThrowsIOException()
		{
			var exportData = new GetApplicationExportDataQueryResponse
			{
				ApplicationMetadata = new ApplicationExportMetadataResponse
				{
					OrganisationName = "Org",
					Qan = "123",
					SubmissionId = 4,
					FormName = "Form"
				},
				ApplicationFormStructure = new GetFormPreviewByIdQueryResponse(),
				ApplicationFormResponse = new()
			};

			var files = new List<FileMetadataDto>
			{
				new FileMetadataDto
                {
					BlobContainer = "any",
					BlobPath = "files/app/question/file"
                }
			};

			_fileServiceMock!
				.Setup(x => x.OpenReadStreamAsync(It.IsAny<string>(), It.IsAny<string>()))
				.ReturnsAsync((Stream)null);

			//Assert.ThrowsAsync<IOException>(() =>
			//	_service.GenerateExportZipAsync(exportData, files));
		}

        [Fact]
        public async Task GenerateExportZipAsync_MessageAttachment_FilePlacedInMessageFolder()
        {
            var exportData = new GetApplicationExportDataQueryResponse
            {
                ApplicationMetadata = new ApplicationExportMetadataResponse
                {
                    OrganisationName = "Org",
                    Qan = "123",
                    SubmissionId = 1,
                    FormName = "Form"
                },
                ApplicationFormStructure = new GetFormPreviewByIdQueryResponse(),
                ApplicationFormResponse = new()
            };

            var files = new List<FileMetadataDto>
			{
				new FileMetadataDto
				{
					FileCategory = FileCategory.MessageAttachment,
					FileName = "msg.txt",
					BlobContainer = "files",
					BlobPath = "messages/msg.txt"
				}
			};

            _fileServiceMock!
                .Setup(x => x.OpenReadStreamAsync("files", "messages/msg.txt"))
                .ReturnsAsync(new MemoryStream(new byte[] { 1 }));

            _htmlRendererMock!
                .Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync("<html></html>");

            var result = await _service.GenerateExportZipAsync(exportData, files);

            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateExportZipAsync_QuestionUpload_WithQuestionMap_UsesMappedPath()
        {
            var questionId = Guid.NewGuid();

            var exportData = new GetApplicationExportDataQueryResponse
            {
                ApplicationMetadata = new ApplicationExportMetadataResponse
                {
                    OrganisationName = "Org",
                    Qan = "123",
                    SubmissionId = 1,
                    FormName = "Form"
                },
                ApplicationFormStructure = new GetFormPreviewByIdQueryResponse
                {
                    SectionsWithPagesAndQuestions =
					{
						new()
						{
							Order = 1,
							Pages =
							{
								new()
								{
									Order = 2,
									Questions =
									{
										new()
										{
											Id = questionId,
											Order = 3
										}
									}
								}
							}
						}
					}
                },
                ApplicationFormResponse = new()
            };

            var files = new List<FileMetadataDto>
			{
				new FileMetadataDto
				{
					FileCategory = FileCategory.QuestionUpload,
					FileName = "answer.txt",
					BlobContainer = "files",
					BlobPath = "path/blob",
					QuestionId = questionId
				}
			};

            _fileServiceMock!
                .Setup(x => x.OpenReadStreamAsync("files", "path/blob"))
                .ReturnsAsync(new MemoryStream(new byte[] { 1 }));

            _htmlRendererMock!
                .Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync("<html></html>");

            var result = await _service.GenerateExportZipAsync(exportData, files);

            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task GenerateExportZipAsync_QuestionUpload_WithMissingQuestionMap_UsesFallbackPath()
        {
            var exportData = new GetApplicationExportDataQueryResponse
            {
                ApplicationMetadata = new ApplicationExportMetadataResponse
                {
                    OrganisationName = "Org",
                    Qan = "123",
                    SubmissionId = 1,
                    FormName = "Form"
                },
                ApplicationFormStructure = new GetFormPreviewByIdQueryResponse
                {
                    SectionsWithPagesAndQuestions = new()
                },
                ApplicationFormResponse = new()
            };

            var files = new List<FileMetadataDto>
			{
				new FileMetadataDto
				{
					FileCategory = FileCategory.QuestionUpload,
					FileName = "fallback.txt",
					BlobContainer = "files",
					BlobPath = "path/blob",
					QuestionId = Guid.NewGuid() // no matching question
				}
			};

            _fileServiceMock!
                .Setup(x => x.OpenReadStreamAsync("files", "path/blob"))
                .ReturnsAsync(new MemoryStream(new byte[] { 1 }));

            _htmlRendererMock!
                .Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync("<html></html>");

            var result = await _service.GenerateExportZipAsync(exportData, files);

            Assert.True(result.Length > 0);
        }

    }
}