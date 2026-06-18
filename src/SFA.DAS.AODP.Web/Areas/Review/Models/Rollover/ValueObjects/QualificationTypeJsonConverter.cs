using System.Text.Json;
using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

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