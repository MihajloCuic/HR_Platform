using System.Text.Json;
using System.Text.Json.Serialization;

namespace HR_Platform.Helpers
{
    /// <summary>
    /// DateOnlyJsonConverter is a custom JSON converter for the DateOnly type.
    /// 
    /// - The default System.Text.Json serializer doesn't know how to handle DateOnly
    /// - This converter tells the serializer how to convert DateOnly to/from JSON
    ///
    /// - Registered in Program.cs to automatically apply to all DateOnly properties
    /// - Converts DateOnly objects to "yyyy-MM-dd" string format when serializing to JSON
    /// - Parses "yyyy-MM-dd" strings back to DateOnly objects when deserializing from JSON
    /// 
    /// Example:
    /// - C# object: birthday = new DateOnly(1995, 10, 19)
    /// - JSON output: "birthday": "1995-10-19"
    /// - JSON input: "birthday": "1995-10-19" → Converts back to DateOnly(1995, 10, 19)
    /// </summary>
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (DateOnly.TryParseExact(value, Format, null, System.Globalization.DateTimeStyles.None, out var date))
            {
                return date;
            }
            throw new JsonException($"Invalid date format. Expected format: {Format}");
        }
        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }
}
