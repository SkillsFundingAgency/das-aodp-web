using Microsoft.AspNetCore.Http;
using Moq;
using SFA.DAS.AODP.Web.Areas.Review.Models.Rollover;
using System.Text;

namespace SFA.DAS.AODP.Web.UnitTests.Helpers.File
{
    public class CsvFileReaderTests
    {
        private ICsvFileReader CreateReader() => new CsvFileReader();

        private IFormFile CreateFormFile(string content, string fileName = "test.csv")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.OpenReadStream()).Returns(stream);

            return file.Object;
        }

        private readonly string[] RequiredHeaders =
        {
            "Qualification number",
            "Title",
            "Awarding organisation",
            "Funding offer",
            "Funding approval end date"
        };

        [Fact]
        public async Task FileReadAsync_ReturnsError_WhenFileIsNull()
        {
            var reader = CreateReader();

            var result = await reader.FileReadAsync<object>(
                null,
                RequiredHeaders,
                row => new object()
            );

            Assert.False(result.IsValid);
            Assert.Contains("You must select a CSV file.", result.Errors);
        }

        [Fact]
        public async Task FileReadAsync_ReturnsError_WhenWrongExtension()
        {
            var reader = CreateReader();
            var file = CreateFormFile("Qualification number,Title", "test.txt");

            var result = await reader.FileReadAsync<object>(
                file,
                RequiredHeaders,
                row => new object()
            );

            Assert.False(result.IsValid);
            Assert.Contains("This must be a CSV file.", result.Errors);
        }

        [Fact]
        public async Task FileReadAsync_ReturnsError_WhenFileIsEmpty()
        {
            var reader = CreateReader();
            var file = CreateFormFile("");

            var result = await reader.FileReadAsync<object>(
                file,
                RequiredHeaders,
                row => new object()
            );

            Assert.False(result.IsValid);
            Assert.Contains("The selected file is empty. Upload a CSV file that contains data.", result.Errors);
        }

        [Fact]
        public async Task FileReadAsync_ReturnsError_WhenRequiredHeaderMissing()
        {
            var reader = CreateReader();

            var file = CreateFormFile(
                        @"Bad1,Bad2
                        123,ABC");

            var result = await reader.FileReadAsync<object>(
                file,
                RequiredHeaders,
                row => new object()
            );

            Assert.False(result.IsValid);
            Assert.Contains("The file you provided does not match the required format.", result.Errors);
        }

        [Fact]
        public async Task FileReadAsync_MapsRows_WhenHeadersValid()
        {
            var reader = CreateReader();

            var file = CreateFormFile(
                        @"Qualification number,Title,Awarding organisation,Funding offer,Funding approval end date
                        12345,Test Qual,Pearson,Yes,2025-12-31
                        99999,Another Qual,City & Guilds,Yes,2026-01-15");

            var result = await reader.FileReadAsync(
                file,
                RequiredHeaders,
                row => new
                {
                    Number = row["qualification number"],
                    Title = row["title"],
                    Ao = row["awarding organisation"],
                    Offer = row["funding offer"],
                    End = row["funding approval end date"]
                }
            );

            Assert.True(result.IsValid);
            Assert.Equal(2, result.Items.Count);

            Assert.Equal("12345", result.Items[0].Number);
            Assert.Equal("Test Qual", result.Items[0].Title);
            Assert.Equal("Pearson", result.Items[0].Ao);
            Assert.Equal("Yes", result.Items[0].Offer);
            Assert.Equal("2025-12-31", result.Items[0].End);
        }

        [Fact]
        public async Task FileReadAsync_NormalizesHeadersCorrectly()
        {
            var reader = CreateReader();

            // Contains BOM, extra spaces, and weird casing
            var file = CreateFormFile(
                @"   ﻿QUALIFICATION   NUMBER   , Title  ,   AWARDING ORGANISATION , Funding Offer, FUNDING APPROVAL   END DATE
                1000,Science,NCFE,Yes,2024-08-01");

            var result = await reader.FileReadAsync(
                file,
                RequiredHeaders,
                row => new
                {
                    Number = row["qualification number"],
                    Title = row["title"],
                    Ao = row["awarding organisation"],
                    Offer = row["funding offer"],
                    End = row["funding approval end date"]
                }
            );

            Assert.True(result.IsValid);
            Assert.Single(result.Items);
            Assert.Equal("1000", result.Items[0].Number);
            Assert.Equal("Science", result.Items[0].Title);
        }

        [Fact]
        public async Task FileReadAsync_FillsMissingColumnsWithEmptyStrings()
        {
            var reader = CreateReader();

            var file = CreateFormFile(
                    @"Qualification number,Title,Awarding organisation,Funding offer,Funding approval end date
                    1,Maths");

            var result = await reader.FileReadAsync(
                file,
                RequiredHeaders,
                row => new
                {
                    Number = row["qualification number"],
                    Title = row["title"],
                    Ao = row["awarding organisation"],   // missing
                    Offer = row["funding offer"],        // missing
                    End = row["funding approval end date"] // missing
                }
            );

            Assert.True(result.IsValid);
            Assert.Single(result.Items);

            Assert.Equal("1", result.Items[0].Number);
            Assert.Equal("Maths", result.Items[0].Title);
            Assert.Equal("", result.Items[0].Ao);
            Assert.Equal("", result.Items[0].Offer);
            Assert.Equal("", result.Items[0].End);
        }
    }
}