using System.Text.Json;
using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Domain.ValueObjects;

public sealed class QualificationLevelJsonConverter : JsonConverter<QualificationLevel>
{
    public override QualificationLevel? Read(
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
                $"Expected string when deserialising {nameof(QualificationLevel)}.");
        }

        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            return QualificationLevel.Unspecified;
        }

        if (QualificationLevel.TryParse(value, out var result))
        {
            return result;
        }

        return QualificationLevel.Unspecified;
    }

    public override void Write(
        Utf8JsonWriter writer,
        QualificationLevel value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}