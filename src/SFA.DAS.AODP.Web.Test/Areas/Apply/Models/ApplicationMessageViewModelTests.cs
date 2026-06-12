using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Areas.Apply.Models;

namespace SFA.DAS.AODP.Web.UnitTests.Areas.Apply.Models;

public class ApplicationMessageViewModelTests : UnitTest
{
    [Fact]
    public void TimelineTitle_ShouldReturnMessageHeader()
    {
        // Arrange
        var sut = CreateViewModel(messageHeader: "Application unlocked");

        // Act
        var result = sut.TimelineTitle;

        // Assert
        result.ShouldBe("Application unlocked");
    }

    [Fact]
    public void TimelineMetadata_ShouldReturnQfauWithFormattedSentAt()
    {
        // Arrange
        var sut = CreateViewModel(
            sentAt: new DateTime(2026, 6, 12, 14, 5, 0));

        // Act
        var result = sut.TimelineMetadata;

        // Assert
        result.ShouldBe("Qualification Funding Approval Unit, 12 Jun 2026 at 14:05");
    }

    [Theory]
    [InlineData("Some message text", true)]
    [InlineData("", false)]
    [InlineData(" ", true)]
    public void ShowText_ShouldReturnWhetherTextHasAnyCharacters(
        string text,
        bool expectedResult)
    {
        // Arrange
        var sut = CreateViewModel(text: text);

        // Act
        var result = sut.ShowText;

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void MessageTypeConfiguration_ShouldReturnConfigurationForMessageType()
    {
        // Arrange
        var messageType = MessageType.InternalNotes;

        var expectedConfiguration = MessageTypeConfigurationRules
            .GetMessageSharingSettings(messageType.ToString());

        var sut = CreateViewModel(messageType: messageType.ToString());

        // Act
        var result = sut.MessageTypeConfiguration;

        // Assert
        result.DisplayName.ShouldBe(expectedConfiguration.DisplayName);
        result.MessageInformationBanner.ShouldBe(expectedConfiguration.MessageInformationBanner);
    }

    [Fact]
    public void MessageTypeDisplay_ShouldReturnDisplayNameFromMessageTypeConfiguration()
    {
        // Arrange
        var messageType = MessageType.InternalNotesForPartners;

        var expectedDisplayName = MessageTypeConfigurationRules
            .GetMessageSharingSettings(messageType.ToString())
            .DisplayName;

        var sut = CreateViewModel(messageType: messageType.ToString());

        // Act
        var result = sut.MessageTypeDisplay;

        // Assert
        result.ShouldBe(expectedDisplayName);
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