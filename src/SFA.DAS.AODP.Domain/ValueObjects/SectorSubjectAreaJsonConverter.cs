using System.Text.Json;
using System.Text.Json.Serialization;

namespace SFA.DAS.AODP.Domain.ValueObjects;

public sealed class SectorSubjectAreaJsonConverter : JsonConverter<SectorSubjectArea>
{
    public override SectorSubjectArea? Read(
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
            return SectorSubjectArea.NotSpecified;
        }

        if (SectorSubjectArea.TryParse(value, out var result))
        {
            return result;
        }

        return SectorSubjectArea.NotSpecified;
    }

    public override void Write(
        Utf8JsonWriter writer,
        SectorSubjectArea value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}