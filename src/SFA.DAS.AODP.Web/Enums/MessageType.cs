using SFA.DAS.AODP.Models.Users;

namespace SFA.DAS.AODP.Web.Enums;

public enum MessageType
{
    UnlockApplication,
    PutApplicationOnHold,

    RequestInformationFromAOByQfau,
    RequestInformationFromAOByOfqaul,
    RequestInformationFromAOBySkillsEngland,

    ReplyToInformationRequest,

    InternalNotes,

    InternalNotesForQfauFromOfqual,
    InternalNotesForQfauFromSkillsEngland,

    InternalNotesForPartners,

    // System Audit
    ApplicationSharedWithOfqual,
    ApplicationSharedWithSkillsEngland,

    ApplicationUnsharedWithOfqual,
    ApplicationUnsharedWithSkillsEngland,

    ApplicationSubmitted
}

public static class MessageTypeConfigurationRules
{
    public static readonly Dictionary<MessageType, Func<MessageTypeConfiguration>> MessageTypeConfigurations =
        new()
        {
            { MessageType.UnlockApplication, () => new MessageTypeConfiguration
            {
                DisplayName = "Unlock Application",
                MessageInformationBanner = "This message will be visible to the Awarding Organisation",
            } },

            { MessageType.PutApplicationOnHold, () => new MessageTypeConfiguration
            {
                DisplayName = "Put Application On Hold",
                MessageInformationBanner = "This message will be visible to the Awarding Organisation",
            } },

            { MessageType.RequestInformationFromAOByQfau, () => new MessageTypeConfiguration
            {
                DisplayName = "Request information from AO",
                MessageInformationBanner = "This message will be visible to Partners and the Awarding Organisation",
            } },

            { MessageType.RequestInformationFromAOByOfqaul, () => new MessageTypeConfiguration
            {
                DisplayName = "Request information from AO",
                MessageInformationBanner = "This message will be visible to Partners and the Awarding Organisation",
            } },

            { MessageType.RequestInformationFromAOBySkillsEngland, () => new MessageTypeConfiguration
            {
                DisplayName = "Request information from AO",
                MessageInformationBanner = "This message will be visible to Partners and the Awarding Organisation",
            } },

            { MessageType.InternalNotes, () => new MessageTypeConfiguration
            {
                DisplayName = "Internal notes",
                MessageInformationBanner = "This message will only be visible to DfE",
            } },

            { MessageType.InternalNotesForQfauFromOfqual, () => new MessageTypeConfiguration
            {
                DisplayName = "Internal notes for DfE",
                MessageInformationBanner = "This message will be visible to DfE",
            } },

            { MessageType.InternalNotesForQfauFromSkillsEngland, () => new MessageTypeConfiguration
            {
                DisplayName = "Internal notes for DfE",
                MessageInformationBanner = "This message will be visible to DfE",
            } },

            { MessageType.InternalNotesForPartners, () => new MessageTypeConfiguration
            {
                DisplayName = "Internal notes for partners",
                MessageInformationBanner = "This message will be visible to DfE, Ofqual and Skills England",
            } },

            { MessageType.ReplyToInformationRequest, () => new MessageTypeConfiguration
            {
                DisplayName = "Reply To Information Request",
            } }
        };

    public static MessageTypeConfiguration GetMessageSharingSettings(string messageTypeString)
    {
        _ = Enum.TryParse(messageTypeString, out MessageType messageTypeEnum) ? messageTypeEnum : MessageType.InternalNotes;

        if (MessageTypeConfigurations.TryGetValue(messageTypeEnum, out var config))
        {
            return config();
        }

        return new MessageTypeConfiguration { };
    }
}

public class MessageTypeConfiguration
{
    public string? DisplayName { get; set; }
    public string? MessageInformationBanner { get; set; }
}