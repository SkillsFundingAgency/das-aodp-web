using SFA.DAS.Aodp.Domain.Files;
using SFA.DAS.AODP.Application.Queries.Application.Form;
using SFA.DAS.AODP.Application.Queries.Application.Review;
using SFA.DAS.AODP.Application.Queries.Files;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Extensions;
using System.IO.Compression;

namespace SFA.DAS.AODP.Web.Helpers.Export
{
    public interface IApplicationExportService
    {
        Task<byte[]> GenerateExportZipAsync(GetApplicationExportDataQueryResponse exportData, List<FileMetadataDto> files);
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
            List<FileMetadataDto> files)
        {
            var metadata = exportData.ApplicationMetadata;
            var form = exportData.ApplicationFormStructure;

            var questionMap = BuildQuestionMap(form);

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var summaryHtml = await GenerateSummaryHtml(exportData, files, questionMap);
                var summaryEntry = archive.CreateEntry(ApplicationExportConstants.SummaryFileName);

                await using (var stream = summaryEntry.Open())
                await using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(summaryHtml);
                }

                foreach (var file in files)
                {
                    bool isMessageFile = file.FileCategory == FileCategory.MessageAttachment;

                    string filePath;

                    if (!isMessageFile)
                    {

                        if (file.QuestionId.HasValue  && questionMap.TryGetValue(file.QuestionId.Value, out var questionReference))
                        {
                            filePath =
                                $"{questionReference}/" +
                                $"{file.FileName.SanitiseFileName()}";
                        }
                        else
                        {
                            filePath = $"{file.FileName.SanitiseFileName()}";
                        }
                    }
                    else
                    {
                        filePath =
                            $"{ApplicationExportConstants.MessageFolderName}/" +
                            $"{file.FileName.SanitiseFileName()}";
                    }

                    await using var fileStream =
                        await _fileService.OpenReadStreamAsync(file.BlobContainer, file.BlobPath)
                        ?? throw new IOException($"Could not open stream for {file.BlobContainer}/{file.BlobPath}");

                    var entry = archive.CreateEntry(filePath);

                    await using var entryStream = entry.Open();

                    await fileStream.CopyToAsync(entryStream);
                }
            }

            return zipStream.ToArray();
        }

        private async Task<string> GenerateSummaryHtml(
            GetApplicationExportDataQueryResponse exportData,
            List<FileMetadataDto> files,
            Dictionary<Guid, string> questionMap)
        {
            var readOnlyVm = ApplicationReadOnlyDetailsViewModel.Map(
                exportData.ApplicationFormStructure,
                exportData.ApplicationFormResponse,
                files);

            var exportSummaryModel = new ApplicationExportViewModel
            {
                ApplicationFormModel = readOnlyVm,
                ApplicationSummaryModel = new ApplicationReadOnlyDetailsSummary(exportData.ApplicationMetadata),
                QuestionMap = questionMap
            };

            return await _htmlExportRenderer.RenderAsync(ApplicationExportConstants.SummaryViewName, exportSummaryModel);
        }

        private Dictionary<Guid, string> BuildQuestionMap(GetFormPreviewByIdQueryResponse form)
        {
            var map = new Dictionary<Guid, string>();

            foreach (var section in form.SectionsWithPagesAndQuestions)
            {
                foreach (var page in section.Pages)
                {
                    foreach (var question in page.Questions)
                    {
                        map[question.Id] =
                            $"{section.Order}.{page.Order}.{question.Order}";
                    }
                }
            }
            return map;
        }
    }
}
