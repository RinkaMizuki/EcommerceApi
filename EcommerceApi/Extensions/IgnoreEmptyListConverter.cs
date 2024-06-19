using System.Text.Json;
using System.Text.Json.Serialization;

namespace EcommerceApi.Extensions
{
    public class IgnoreEmptyListConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<List<T>>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            if (value == null || value.Count == 0)
            {
                writer.WriteNullValue();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}
