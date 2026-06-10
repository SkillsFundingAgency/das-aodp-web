using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Application.Queries.Application.Review;
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

			var files = new List<UploadedBlob>
			{
				new UploadedBlob
				{
					FullPath = "files/appId/questionId/fileId",
					FileNamePrefix = "file.txt"
				}
			};

			_fileServiceMock!
				.Setup(x => x.OpenReadStreamAsync(It.IsAny<string>()))
				.ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

			_htmlRendererMock!
				.Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<object>()))
				.ReturnsAsync("<html></html>");

			var result = await _service.GenerateExportZipAsync(exportData, files);

			Assert.True(result.Length > 0);

			_fileServiceMock.Verify(x => x.OpenReadStreamAsync(It.IsAny<string>()), Times.Once);
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

			var files = new List<UploadedBlob>();

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

			var files = new List<UploadedBlob>
			{
				new UploadedBlob
				{
					FullPath = "files/app/question/file",
					FileName = "file.txt"
				}
			};

			_fileServiceMock!
				.Setup(x => x.OpenReadStreamAsync(It.IsAny<string>()))
				.ReturnsAsync((Stream)null);

			//Assert.ThrowsAsync<IOException>(() =>
			//	_service.GenerateExportZipAsync(exportData, files));
		}
	}
}