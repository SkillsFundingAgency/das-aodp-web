using System.Text.Json;
using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

[JsonConverter(typeof(QualificationTypeJsonConverter))]
public record QualificationType
{
    public static readonly QualificationType None = new(0, "None");
    public static readonly QualificationType AccessToHigherEducation = new(1, "Access to Higher Education");
    public static readonly QualificationType AdvancedExtensionAward = new(2, "Advanced Extension Award");
    public static readonly QualificationType AlternativeAcademicQualification = new(3, "Alternative Academic Qualification");
    public static readonly QualificationType DigitalFunctionalSkillsQualification = new(4, "Digital Functional Skills Qualification");
    public static readonly QualificationType EnglishForSpeakersOfOtherLanguages = new(5, "English For Speakers of Other Languages");
    public static readonly QualificationType EssentialDigitalSkills = new(6, "Essential Digital Skills");
    public static readonly QualificationType FunctionalSkills = new(7, "Functional Skills");
    public static readonly QualificationType GCEAlevel = new(8, "GCE A Level");
    public static readonly QualificationType GCEASLevel = new(9, "GCE AS Level");
    public static readonly QualificationType GCSE9To1 = new(10, "GCSE (9 to 1)");
    public static readonly QualificationType OccupationalQualification = new(11, "Occupational Qualification");
    public static readonly QualificationType OtherGeneralQualification = new(12, "Other General Qualification");
    public static readonly QualificationType OtherLifeSkillsQualification = new(13, "Other Life Skills Qualification");
    public static readonly QualificationType OtherVocationalQualification = new(14, "Other Vocational Qualification");
    public static readonly QualificationType PerformingArtsGradedExamination = new(15, "Performing Arts Graded Examination");
    public static readonly QualificationType PrincipalLearning = new(16, "Principal Learning");
    public static readonly QualificationType Project = new(17, "Project");
    public static readonly QualificationType TechnicalOccupationQualification = new(18, "Technical Occupation Qualification");
    public static readonly QualificationType TechnicalQualification = new(19, "Technical Qualification");
    public static readonly QualificationType VocationallyRelatedQualification = new(20, "Vocationally-Related Qualification");
    public static readonly QualificationType Unknown = new(99, "Unknown");

    public int Id { get; }
    public string Name { get; }

    private QualificationType(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static readonly IReadOnlyCollection<QualificationType> All = new List<QualificationType>
    {
        AccessToHigherEducation,
        AdvancedExtensionAward,
        AlternativeAcademicQualification,
        DigitalFunctionalSkillsQualification,
        EnglishForSpeakersOfOtherLanguages,
        EssentialDigitalSkills,
        FunctionalSkills,
        GCEAlevel,
        GCEASLevel,
        GCSE9To1,
        OccupationalQualification,
        OtherGeneralQualification,
        OtherLifeSkillsQualification,
        OtherVocationalQualification,
        PerformingArtsGradedExamination,
        PrincipalLearning,
        Project,
        TechnicalOccupationQualification,
        TechnicalQualification,
        VocationallyRelatedQualification
    }.OrderBy(o => o.Name).ToList();

    public static bool TryParse(string? value, out QualificationType? result)
    {
        result = All.SingleOrDefault(x => string.Equals(x.Name, value, StringComparison.OrdinalIgnoreCase));
        return result is not null;
    }

    public override string ToString() => Name;
}

public sealed class QualificationTypeJsonConverter : JsonConverter<QualificationType>
{
    public override QualificationType? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                $"Expected string when deserialising {nameof(QualificationType)}.");
        }

        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            return QualificationType.None;
        }

        if (QualificationType.TryParse(value, out var result))
        {
            return result;
        }

        return QualificationType.Unknown;
    }

    public override void Write(
        Utf8JsonWriter writer,
        QualificationType value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}