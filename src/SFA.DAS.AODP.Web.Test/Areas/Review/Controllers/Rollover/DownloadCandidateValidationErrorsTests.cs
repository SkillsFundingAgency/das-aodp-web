using Microsoft.AspNetCore.Mvc;
using Moq;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Controllers
{
    public class DownloadCandidateValidationErrorsTests:RolloverControllerTestBase
    {

        [Fact]
        public async Task DownloadCandidateValidationErrors_WhenTokenNullOrEmpty_ReturnsBadRequest()
        {
            var controller = CreateController(CreateEmptySession());

            var result1 = await controller.DownloadCandidateValidationErrors(null);
            var result2 = await controller.DownloadCandidateValidationErrors("");

            Assert.IsType<BadRequestResult>(result1);
            Assert.IsType<BadRequestResult>(result2);
        }

        [Fact]
        public async Task DownloadCandidateValidationErrors_WhenCacheReturnsNull_ReturnsNotFound()
        {
            var controller = CreateController(CreateEmptySession());

            CacheServiceMock
                .Setup(x => x.GetAsync<byte[]>(It.IsAny<string>()))
                .ReturnsAsync((byte[]?)null);

            var result = await controller.DownloadCandidateValidationErrors("abc123");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DownloadCandidateValidationErrors_WhenCacheReturnsEmptyArray_ReturnsNotFound()
        {
            var controller = CreateController(CreateEmptySession());

            CacheServiceMock
                .Setup(x => x.GetAsync<byte[]>(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<byte>());

            var result = await controller.DownloadCandidateValidationErrors("abc123");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DownloadCandidateValidationErrors_WhenValidFileFound_ReturnsFileResult()
        {
            var controller = CreateController(CreateEmptySession());

            var bytes = new byte[] { 1, 2, 3 };

            CacheServiceMock
                .Setup(x => x.GetAsync<byte[]>(It.IsAny<string>()))
                .ReturnsAsync(bytes);

            var result = await controller.DownloadCandidateValidationErrors("abc123");

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", file.ContentType);
            Assert.Equal("validation-errors.csv", file.FileDownloadName);
            Assert.Equal(bytes, file.FileContents);

            CacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
        }


    }
}
