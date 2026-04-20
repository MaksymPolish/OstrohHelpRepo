using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Converters;


/// Custom JSON converter that serializes byte[] as Base64 string and vice versa.
/// This is essential for SignalR communication where binary data must be transmitted as text.
/// 
/// Example:
/// - Write (server → client): new byte[] { 1, 2, 3 } → "AQID"
/// - Read (client → server): "AQID" → new byte[] { 1, 2, 3 }

public class ByteArrayToBase64Converter : JsonConverter<byte[]>
{
    /// Deserializes Base64 string to byte array
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var base64String = reader.GetString();
            if (string.IsNullOrEmpty(base64String))
            {
                return null;
            }

            try
            {
                return Convert.FromBase64String(base64String);
            }
            catch (FormatException ex)
            {
                throw new JsonException($"Invalid Base64 string: {base64String}", ex);
            }
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing byte array.");
    }
    /// Serializes byte array to Base64 string
    public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var base64String = Convert.ToBase64String(value);
        writer.WriteStringValue(base64String);
    }
}
