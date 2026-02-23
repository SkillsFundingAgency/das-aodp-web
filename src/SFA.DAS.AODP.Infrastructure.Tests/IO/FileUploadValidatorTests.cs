using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.AODP.Infrastructure.Common.IO;
using SFA.DAS.AODP.Models.Common;
using SFA.DAS.AODP.Models.Exceptions;
using SFA.DAS.AODP.Models.Settings;
using System.Text;
using Xunit;

namespace SFA.DAS.AODP.Infrastructure.UnitTests.Common.IO
{
    public class FileUploadValidatorTests
    {
        private const string ValidFileName = "test.pdf";
        private const string InvalidFileName = ".";
        private const string DisallowedFileName = "test.exe";
        private const string EmptyFileName = "";
        private const string AllowedExtension = ".pdf";
        private const int MaxUploadSizeMb = 1;
        private const int OversizedBytes = (MaxUploadSizeMb * 1024 * 1024) + 1;

        private readonly FileUploadValidator _sut;

        public FileUploadValidatorTests()
        {
            var settings = new FormBuilderSettings
            {
                MaxUploadFileSize = MaxUploadSizeMb,
                UploadFileTypesAllowed = new List<string> { AllowedExtension }
            };

            _sut = new FileUploadValidator(settings);
        }

        [Fact]
        public void ValidateOrThrow_ValidFile_DoesNotThrowAndResetsPosition()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("valid"));

            using var scope = new AssertionScope();

            Action act = () => _sut.ValidateOrThrow(ValidFileName, stream);

            act.Should().NotThrow();
            stream.Position.Should().Be(0);
        }

        [Fact]
        public void ValidateOrThrow_NullStream_ThrowsMissingFile()
        {
            using var scope = new AssertionScope();

            var ex = Assert.Throws<FileUploadPolicyException>(() =>
                _sut.ValidateOrThrow(ValidFileName, null));

            ex.Reason.Should().Be(FileUploadRejectionReason.MissingFile);
        }

        [Fact]
        public void ValidateOrThrow_EmptyFileName_ThrowsMissingFileName()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("data"));

            using var scope = new AssertionScope();

            var ex = Assert.Throws<FileUploadPolicyException>(() =>
                _sut.ValidateOrThrow(EmptyFileName, stream));

            ex.Reason.Should().Be(FileUploadRejectionReason.MissingFileName);
        }

        [Fact]
        public void ValidateOrThrow_InvalidFileName_ThrowsInvalidFileName()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("data"));

            using var scope = new AssertionScope();

            var ex = Assert.Throws<FileUploadPolicyException>(() =>
                _sut.ValidateOrThrow(InvalidFileName, stream));

            ex.Reason.Should().Be(FileUploadRejectionReason.InvalidFileName);
        }

        [Fact]
        public void ValidateOrThrow_DisallowedExtension_ThrowsFileTypeNotAllowed()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("data"));

            using var scope = new AssertionScope();

            var ex = Assert.Throws<FileUploadPolicyException>(() =>
                _sut.ValidateOrThrow(DisallowedFileName, stream));

            ex.Reason.Should().Be(FileUploadRejectionReason.FileTypeNotAllowed);
        }

        [Fact]
        public void ValidateOrThrow_EmptyStream_ThrowsEmptyFile()
        {
            var stream = new MemoryStream();

            using var scope = new AssertionScope();

            var ex = Assert.Throws<FileUploadPolicyException>(() =>
                _sut.ValidateOrThrow(ValidFileName, stream));

            ex.Reason.Should().Be(FileUploadRejectionReason.EmptyFile);
        }

        [Fact]
        public void ValidateOrThrow_OversizedStream_ThrowsFileTooLarge()
        {
            var stream = new MemoryStream(new byte[OversizedBytes]);

            using var scope = new AssertionScope();

            var ex = Assert.Throws<FileUploadPolicyException>(() =>
                _sut.ValidateOrThrow(ValidFileName, stream));

            ex.Reason.Should().Be(FileUploadRejectionReason.FileTooLarge);
        }

        [Fact]
        public void ValidateOrThrow_NonSeekableStream_ThrowsUnknownFileSize()
        {
            var stream = new NonSeekableStream(new MemoryStream(Encoding.UTF8.GetBytes("data")));

            using var scope = new AssertionScope();

            var ex = Assert.Throws<FileUploadPolicyException>(() =>
                _sut.ValidateOrThrow(ValidFileName, stream));

            ex.Reason.Should().Be(FileUploadRejectionReason.UnknownFileSize);
        }

        private sealed class NonSeekableStream : Stream
        {
            private readonly Stream _inner;

            public NonSeekableStream(Stream inner) => _inner = inner;

            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => _inner.CanWrite;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }
            public override void Flush() => _inner.Flush();
            public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => _inner.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
        }
    }
}
