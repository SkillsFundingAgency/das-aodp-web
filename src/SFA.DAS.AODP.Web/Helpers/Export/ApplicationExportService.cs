using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Application.Queries.Application.Review;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Extensions;
using System.IO.Compression;

namespace SFA.DAS.AODP.Web.Helpers.Export
{
    public interface IApplicationExportService
    {
        Task<byte[]> GenerateExportZipAsync(GetApplicationExportDataQueryResponse exportData, List<UploadedBlob> files);
    }

    public class ApplicationExportService : IApplicationExportService
    {
        private readonly IFileService _fileService;
        private readonly IHtmlExportRenderer _htmlExportRenderer;

        public ApplicationExportService(IFileService fileService, IHtmlExportRenderer htmlExportRenderer)
        {
            _fileService = fileService;
            _htmlExportRenderer = htmlExportRenderer;
        }

        public async Task<byte[]> GenerateExportZipAsync(
            GetApplicationExportDataQueryResponse exportData,
            List<UploadedBlob> files)
        {
            var metadata = exportData.ApplicationMetadata;
            var form = exportData.ApplicationFormStructure;

            string organisationFolder = metadata.OrganisationName.SanitiseFileName();
            string qanFolder =
                string.IsNullOrWhiteSpace(metadata.Qan)
                    ? ApplicationExportConstants.NoQanFolderName
                    : metadata.Qan.SanitiseFileName();
            string applicationFolder = ($"{metadata.SubmissionId.ToString().PadLeft(6, '0')}_{metadata.FormName.SanitiseFileName()}");

            string basePath = $"{organisationFolder}/{qanFolder}/{applicationFolder}";

            var questionMap = BuildQuestionMap(form);

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var summaryHtml = await GenerateSummaryHtml(exportData, files);
                var summaryEntry = archive.CreateEntry($"{basePath}/{ApplicationExportConstants.SummaryFileName}");

                await using (var stream = summaryEntry.Open())
                await using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(summaryHtml);
                }

                foreach (var file in files)
                {
                    bool isMessageFile = file.FullPath.StartsWith($"{ApplicationExportConstants.MessageFolderName}/", StringComparison.OrdinalIgnoreCase);

                    string filePath;

                    if (!isMessageFile)
                    {
                        var questionId = ExtractQuestionIdFromBlobPath(file.FullPath);

                        if (questionId != null && questionMap.TryGetValue(questionId.Value, out var info))
                        {
                            var (sectionTitle, pageTitle, questionTitle) = info;

                            filePath =
                                $"{basePath}/" +
                                $"{sectionTitle.SanitiseFileName()}/" +
                                $"{pageTitle.SanitiseFileName()}/" +
                                $"{questionTitle.SanitiseFileName()}/" +
                                $"{file.FileNameWithPrefix.SanitiseFileName()}";
                        }
                        else
                        {
                            filePath = $"{basePath}/{file.FileNameWithPrefix.SanitiseFileName()}";
                        }
                    }
                    else
                    {
                        filePath = $"{basePath}/{file.FileNameWithPrefix.SanitiseFileName()}";
                    }

                    await using var fileStream =
                        await _fileService.OpenReadStreamAsync(file.FullPath)
                        ?? throw new IOException($"Could not open stream for {file.FullPath}");

                    var entry = archive.CreateEntry(filePath);

                    await using var entryStream = entry.Open();

                    await fileStream.CopyToAsync(entryStream);
                }
            }

            return zipStream.ToArray();
        }

        private async Task<string> GenerateSummaryHtml(
            GetApplicationExportDataQueryResponse exportData,
            List<UploadedBlob> files)
        {
            var readOnlyVm = ApplicationReadOnlyDetailsViewModel.Map(
                exportData.ApplicationFormStructure,
                exportData.ApplicationFormResponse,
                files);

            var exportSummaryModel = new ApplicationExportViewModel
            {
                ApplicationFormModel = readOnlyVm,
                ApplicationSummaryModel = new ApplicationReadOnlyDetailsSummary(exportData.ApplicationMetadata)
            };

            return await _htmlExportRenderer.RenderAsync(ApplicationExportConstants.SummaryViewName, exportSummaryModel);
        }

        private Guid? ExtractQuestionIdFromBlobPath(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return null;

            if (fullPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(fullPath);
                fullPath = uri.AbsolutePath.TrimStart('/');
            }

            var parts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // expected:
            // 0 = container ("files")
            // 1 = applicationId
            // 2 = questionId
            // 3 = fileId

            if (parts.Length < 2)
                return null;

            return Guid.TryParse(parts[1], out var questionId)
                ? questionId
                : null;
        }

        private Dictionary<Guid, (string Section, string Page, string Question)> BuildQuestionMap(GetFormPreviewByIdQueryResponse form)
        {
            var map = new Dictionary<Guid, (string Section, string Page, string Question)>();

            foreach (var section in form.SectionsWithPagesAndQuestions)
            {
                foreach (var page in section.Pages)
                {
                    foreach (var question in page.Questions)
                    {
                        map[question.Id] = (section.Title, page.Title, question.Title);
                    }
                }
            }

            return map;
        }

    }
}
