using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avro
{
    internal class AvroFieldTypeSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch(reader.TokenType)
            {
                case JsonToken.String:
                    return (string)reader.Value;
                case JsonToken.StartObject:
                    return serializer.Deserialize<AvroSchema>(reader);
                case JsonToken.StartArray:
                    
                default:
                    throw new NotImplementedException(reader.TokenType.ToString());
            }
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
