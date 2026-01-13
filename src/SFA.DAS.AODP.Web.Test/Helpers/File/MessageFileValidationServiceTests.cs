using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Moq;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Helpers.File;

namespace SFA.DAS.AODP.Web.UnitTests.Helpers.File
{
    public class MessageFileValidationServiceTests
    {
        private readonly FormBuilderSettings _formBuilderSettings = new();
        private readonly MessageFileValidationService _sut;

        public MessageFileValidationServiceTests() => _sut = new(_formBuilderSettings);

        [Fact]
        public void ValidateFiles_TooManyFiles_ErrorThrown()
        {
            // Arrange
            _formBuilderSettings.MaxUploadNumberOfFiles = 1;
            List<IFormFile> files = [Mock.Of<IFormFile>(), Mock.Of<IFormFile>()];

            // Act
            var ex = Assert.Throws<Exception>(() => _sut.ValidateFiles(files));

            // Assert
            Assert.Equal($"Cannot upload more than {_formBuilderSettings.MaxUploadNumberOfFiles} files", ex.Message);
        }

        [Fact]
        public void ValidateFiles_FileTooBig_ErrorThrown()
        {
            // Arrange
            _formBuilderSettings.MaxUploadNumberOfFiles = 1;
            _formBuilderSettings.MaxUploadFileSize = 1;
            Mock<IFormFile> file = new();
            List<IFormFile> files = [file.Object];

            file.SetupGet(f => f.Length).Returns(2 * 1024 * 1024);

            // Act
            var ex = Assert.Throws<Exception>(() => _sut.ValidateFiles(files));

            // Assert
            Assert.Equal($"File size exceeds max allowed size of {_formBuilderSettings.MaxUploadFileSize}mb.", ex.Message);
        }

        [Fact]
        public void ValidateFiles_FileWrongExtension_ErrorThrown()
        {
            // Arrange
            _formBuilderSettings.MaxUploadNumberOfFiles = 1;
            _formBuilderSettings.MaxUploadFileSize = 1;
            _formBuilderSettings.UploadFileTypesAllowed = [".DOCX"];

            Mock<IFormFile> file = new();
            List<IFormFile> files = [file.Object];

            file.SetupGet(f => f.Length).Returns(1);
            file.SetupGet(f => f.FileName).Returns("Test.pdf"); 

            // Act
            var ex = Assert.Throws<Exception>(() => _sut.ValidateFiles(files));

            // Assert
            Assert.Equal($"File type is not included in the allowed file types: {string.Join(",", _formBuilderSettings.UploadFileTypesAllowed)}", ex.Message);
        }

        [Fact]
        public void ValidateFiles_FileValid_NoErrorThrown()
        {
            // Arrange
            _formBuilderSettings.MaxUploadNumberOfFiles = 1;
            _formBuilderSettings.MaxUploadFileSize = 1;
            _formBuilderSettings.UploadFileTypesAllowed = [".DOCX"];

            Mock<IFormFile> file = new();
            List<IFormFile> files = [file.Object];

            file.SetupGet(f => f.Length).Returns(1);
            file.SetupGet(f => f.FileName).Returns("Test.docx");

            // Act
            _sut.ValidateFiles(files);

        }

        private MessageFileValidationService CreateService(FormBuilderSettings? settings = null)
        {
            settings ??= new FormBuilderSettings
            {
                MaxUploadNumberOfFiles = 5,
                MaxUploadFileSize = 5,
                UploadFileTypesAllowed = new List<string> { ".xlsx" }
            };
            return new MessageFileValidationService(settings);
        }

        private byte[] CreateExcelBytes(string sheetName, string[] headers, bool includeDataRow = true)
        {
            using var ms = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var sharedStringPart = workbookPart.AddNewPart<SharedStringTablePart>();
                sharedStringPart.SharedStringTable = new SharedStringTable();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                var worksheet = new Worksheet(sheetData);
                worksheetPart.Worksheet = worksheet;

                // Header row at index 1
                var headerRow = new Row() { RowIndex = 1 };
                for (int i = 0; i < headers.Length; i++)
                {
                    int sstIndex = AddSharedStringItem(sharedStringPart.SharedStringTable, headers[i]);
                    var cell = new Cell()
                    {
                        CellReference = GetCellReference(i, 1),
                        DataType = CellValues.SharedString,
                        CellValue = new CellValue(sstIndex.ToString())
                    };
                    headerRow.AppendChild(cell);
                }
                sheetData.AppendChild(headerRow);

                if (includeDataRow)
                {
                    var dataRow = new Row() { RowIndex = 2 };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var value = $"val{i + 1}";
                        int sstIndex = AddSharedStringItem(sharedStringPart.SharedStringTable, value);
                        var cell = new Cell()
                        {
                            CellReference = GetCellReference(i, 2),
                            DataType = CellValues.SharedString,
                            CellValue = new CellValue(sstIndex.ToString())
                        };
                        dataRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(dataRow);
                }

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                var sheet = new Sheet()
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1U,
                    Name = sheetName
                };
                sheets.Append(sheet);

                workbookPart.Workbook.Save();
            }

            return ms.ToArray();

            static int AddSharedStringItem(SharedStringTable sst, string text)
            {
                var existing = sst.Elements<SharedStringItem>()
                                  .Select((item, index) => new { item, index })
                                  .FirstOrDefault(x => x.item.InnerText == text);
                if (existing != null && existing.item != null)
                {
                    return existing.index;
                }
                sst.AppendChild(new SharedStringItem(new Text(text)));
                return sst.Elements<SharedStringItem>().Count() - 1;
            }

            static string GetCellReference(int columnIndex, int rowIndex)
            {
                return $"{ColumnNameFromIndex(columnIndex)}{rowIndex}";
            }

            static string ColumnNameFromIndex(int index)
            {
                const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var result = string.Empty;
                index++; // 1-based
                while (index > 0)
                {
                    var modulo = (index - 1) % 26;
                    result = letters[modulo] + result;
                    index = (index - modulo) / 26;
                }
                return result;
            }
        }

        private IFormFile CreateFormFileFromBytes(byte[] bytes, string fileName)
        {
            var ms = new MemoryStream(bytes);
            return new FormFile(ms, 0, ms.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        [Fact]
        public async Task ValidateImportFile_ReturnsFalse_When_RequestIsInvalid()
        {
            var service = CreateService();
            var result = await service.ValidateImportFile(null, null, Array.Empty<string>(), "Sheet1", 0, 1, _ => new { }, CancellationToken.None);
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateImportFile_ReturnsFalse_When_SheetIsMissing()
        {
            var service = CreateService();
            var bytes = CreateExcelBytes("PresentSheet", new[] { "H1", "H2" }, includeDataRow: true);
            var formFile = CreateFormFileFromBytes(bytes, "file.xlsx");

            var result = await service.ValidateImportFile(
                formFile,
                "file.xlsx",
                new[] { "H1" },
                targetSheetName: "MissingSheet",
                defaultRowIndex: 0,
                minMatches: 1,
                mapColumns: map => new { H1 = map.ContainsKey("H1") ? map["H1"] : string.Empty },
                cancellationToken: CancellationToken.None);

            Assert.False(result);
        }

        [Fact]
        public async Task ValidateImportFile_ReturnsFalse_When_OnlyHeaderRowExists()
        {
            var service = CreateService();
            var bytes = CreateExcelBytes("Sheet1", new[] { "H1", "H2" }, includeDataRow: false);
            var formFile = CreateFormFileFromBytes(bytes, "file.xlsx");

            var result = await service.ValidateImportFile(
                formFile,
                "file.xlsx",
                new[] { "H1" },
                targetSheetName: "Sheet1",
                defaultRowIndex: 0,
                minMatches: 1,
                mapColumns: map => new { H1 = map.ContainsKey("H1") ? map["H1"] : string.Empty },
                cancellationToken: CancellationToken.None);

            Assert.False(result);
        }

        [Fact]
        public async Task ValidateImportFile_ReturnsFalse_When_MappedColumnsMissing()
        {
            var service = CreateService();
            var headers = new[] { "H1", "H2" };
            var bytes = CreateExcelBytes("Sheet1", headers, includeDataRow: true);
            var formFile = CreateFormFileFromBytes(bytes, "file.xlsx");

            object MapWithMissing(IDictionary<string, string> map)
            {
                return new
                {
                    H1 = map.ContainsKey("H1") ? map["H1"] : string.Empty,
                    H2 = string.Empty
                };
            }

            var result = await service.ValidateImportFile(
                formFile,
                "file.xlsx",
                headerKeywords: new[] { "H1", "H2" },
                targetSheetName: "Sheet1",
                defaultRowIndex: 0,
                minMatches: 1,
                mapColumns: MapWithMissing,
                cancellationToken: CancellationToken.None);

            Assert.False(result);
        }

        [Fact]
        public async Task ValidateImportFile_ReturnsTrue_When_ValidFileAndMapping()
        {
            var service = CreateService();
            var headers = new[] { "H1", "H2", "H3" };
            var bytes = CreateExcelBytes("Sheet1", headers, includeDataRow: true);
            var formFile = CreateFormFileFromBytes(bytes, "file.xlsx");

            object MapValid(IDictionary<string, string> map)
            {
                return new
                {
                    H1 = map.ContainsKey("H1") ? map["H1"] : "A",
                    H2 = map.ContainsKey("H2") ? map["H2"] : "B",
                    H3 = map.ContainsKey("H3") ? map["H3"] : "C"
                };
            }

            var result = await service.ValidateImportFile(
                formFile,
                "file.xlsx",
                headerKeywords: new[] { "H1", "H2", "H3" },
                targetSheetName: "Sheet1",
                defaultRowIndex: 0,
                minMatches: 1,
                mapColumns: MapValid,
                cancellationToken: CancellationToken.None);

            Assert.True(result);
        }
    }
}
