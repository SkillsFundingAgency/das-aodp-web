using SFA.DAS.AODP.Models.Users;

namespace SFA.DAS.AODP.Web.Enums;

public enum MessageType
{
    UnlockApplication, // DfE only
    PutApplicationOnHold, // DfE only

    RequestInformationFromAOByQfau,
    RequestInformationFromAOByOfqaul,
    RequestInformationFromAOBySkillsEngland,

    ReplyToInformationRequest, // AO only

    InternalNotes, // DfE to DfE

    InternalNotesForQfauFromOfqual,
    InternalNotesForQfauFromSkillsEngland,

    InternalNotesForPartners
}

// FOR POSTING

public static class SendMessageTypeRule
{
    public static Dictionary<(UserType, string), string> Rules { get; } = new()
    {
         // DfE 
        { (UserType.Qfau, "RequestInformation"), "RequestInformationFromAOByQfau" },
        { (UserType.Qfau, "UnlockApplication"), "UnlockApplication" },
        { (UserType.Qfau, "PutApplicationOnHold"), "PutApplicationOnHold" },
        { (UserType.Qfau, "InternalNotes"), "InternalNotes" },
        { (UserType.Qfau, "InternalNotesForPartners"), "InternalNotesForPartners" },

         // Ofqual 
        { (UserType.Ofqual, "RequestInformation"), "RequestInformationFromAOByOfqaul" },
        { (UserType.Ofqual,  "InternalNotesForDfE"), "InternalNotesForQfauFromOfqual" },

        // SkillsEngland
        { (UserType.SkillsEngland, "RequestInformation"), "RequestInformationFromAOBySkillsEngland" },
        { (UserType.SkillsEngland,  "InternalNotesForDfE"), "InternalNotesForQfauFromSkillsEngland" },

        // AO
        { (UserType.AwardingOrganisation,  "ReplyToInformationRequest"), "ReplyToInformationRequest" },
    };

    public static string GetMessageType(UserType userType, string messageType)
    {
        if (Rules.TryGetValue((userType, messageType), out var messageTypeToSend))
        {
            return messageTypeToSend;
        }

        return "Uknown message type to send";
    }
}


// FOR VIEWING

public static class MessageTypeConfigurationRules
{
    public static readonly Dictionary<MessageType, Func<MessageTypeConfiguration>> MessageTypeConfigurations =
        new()
        {
            // DfE
            { MessageType.UnlockApplication, () => new MessageTypeConfiguration
            {
                DisplayName = "Unlock Application",
                MessageHeader = "Application Unlocked",
                SharedWithDfe = true,
                SharedWithOfqual = false,
                SharedWithSkillsEngland = false,
                SharedWithAwardingOrganisation = true } },

            { MessageType.PutApplicationOnHold, () => new MessageTypeConfiguration
            {
                DisplayName = "Put Application On Hold",
                MessageHeader = "Application Put On Hold",
                SharedWithDfe = true,
                SharedWithOfqual = false,
                SharedWithSkillsEngland = false,
                SharedWithAwardingOrganisation = true } },

            { MessageType.RequestInformationFromAOByQfau, () => new MessageTypeConfiguration
            {
                DisplayName = "Request Information",
                MessageHeader = "Information Requested From Awarding Organisation",
                SharedWithDfe = true,
                SharedWithOfqual = false,
                SharedWithSkillsEngland = false,
                SharedWithAwardingOrganisation = true } },

            { MessageType.RequestInformationFromAOByOfqaul, () => new MessageTypeConfiguration
            {
                DisplayName = "Request Information",
                MessageHeader = "Information Requested From Awarding Organisation",
                SharedWithDfe = false,
                SharedWithOfqual = true,
                SharedWithSkillsEngland = false,
                SharedWithAwardingOrganisation = true } },

            { MessageType.RequestInformationFromAOBySkillsEngland, () => new MessageTypeConfiguration
            {
                DisplayName = "Request Information From AO",
                MessageHeader = "Information Requested From Awarding Organisation",
                SharedWithDfe = false,
                SharedWithOfqual = false,
                SharedWithSkillsEngland = true,
                SharedWithAwardingOrganisation = true } },

            { MessageType.InternalNotes, () => new MessageTypeConfiguration
            {
                DisplayName = "Internal Notes",
                MessageHeader = "Internal Note",
                SharedWithDfe = true,
                SharedWithOfqual = false,
                SharedWithSkillsEngland = false,
                SharedWithAwardingOrganisation = false } },

            { MessageType.InternalNotesForQfauFromOfqual, () => new MessageTypeConfiguration
            {
                DisplayName = "Internal Notes for DfE",
                MessageHeader = "Internal Note for DfE",
                SharedWithDfe = true,
                SharedWithOfqual = true,
                SharedWithSkillsEngland = false,
                SharedWithAwardingOrganisation = false } },

            { MessageType.InternalNotesForQfauFromSkillsEngland, () => new MessageTypeConfiguration
            {
                DisplayName = "Internal Notes for DfE",
                MessageHeader = "Internal Note for DfE",
                SharedWithDfe = true,
                SharedWithOfqual = false,
                SharedWithSkillsEngland = true,
                SharedWithAwardingOrganisation = false } },

            { MessageType.InternalNotesForPartners, () => new MessageTypeConfiguration
            {
                DisplayName = "Internal Notes for Partners",
                MessageHeader = "Internal Note for Partners",
                SharedWithDfe = true,
                SharedWithOfqual = true,
                SharedWithSkillsEngland = true,
                SharedWithAwardingOrganisation = false } },

            { MessageType.ReplyToInformationRequest, () => new MessageTypeConfiguration
            {
                DisplayName = "Reply To Information Request",
                MessageHeader = "Answer to Information Request from Awarding Organisation",
                SharedWithDfe = true,
                SharedWithOfqual = true,
                SharedWithSkillsEngland = true,
                SharedWithAwardingOrganisation = true } }
        };

    public static MessageTypeConfiguration GetMessageSharingSettings(MessageType messageType)
    {
        if (MessageTypeConfigurations.TryGetValue(messageType, out var config))
        {
            return config();
        }

        return new MessageTypeConfiguration { };
    }
}

public class MessageTypeConfiguration
{
    public string DisplayName { get; set; }
    public string MessageHeader { get; set; }
    public bool SharedWithDfe { get; set; }
    public bool SharedWithAwardingOrganisation { get; set; }
    public bool SharedWithSkillsEngland { get; set; }
    public bool SharedWithOfqual { get; set; }
}

public class MessageTypeConfigurationA
{
    public string DisplayName { get; set; }
    public bool VisibleToDfe { get; set; }
    public bool VisibleToOfqual { get; set; }
    public bool VisibleToSkillsEngland { get; set; }
    public bool VisibleToAwardingOrganisation { get; set; }
}