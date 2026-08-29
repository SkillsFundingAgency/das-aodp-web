using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationMessage;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Review.Models.ApplicationMessage;

public class ApplicationMessageViewModelTests : UnitTest
{
    [Theory]
    [InlineData(nameof(MessageType.InternalNotes))]
    [InlineData(nameof(MessageType.ReplyToInformationRequest))]
    [InlineData(nameof(MessageType.ApplicationSubmitted))]
    public void TimelineMetadata_ShouldAlwaysReturnSentByNameRegardlessOfMessageType(string messageType)
    {
        // Arrange
        var sut = CreateViewModel(
            sentAt: new DateTime(2026, 6, 12, 14, 5, 0),
            messageType: messageType);

        // Act
        var result = sut.TimelineMetadata;

        // Assert
        result.ShouldBe("Test User, 12 Jun 2026 at 14:05");
    }

    private static ApplicationMessageViewModel CreateViewModel(
        string? messageHeader = null,
        string? text = null,
        DateTime? sentAt = null,
        string? messageType = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            MessageHeader = messageHeader ?? "Test message header",
            Text = text ?? "Test message text",
            SentAt = sentAt ?? new DateTime(2026, 6, 12, 14, 5, 0),
            SentByName = "Test User",
            SentByEmail = "test@example.com",
            MessageType = messageType ?? nameof(MessageType.InternalNotes)
        };
}
