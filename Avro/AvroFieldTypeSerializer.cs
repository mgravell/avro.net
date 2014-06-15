using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Avro
{
    internal class AvroTypeSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>(StringComparer.Ordinal);
            if (reader.TokenType != JsonToken.StartObject) throw new InvalidOperationException("Expecting JsonToken.StartObject");
            
            while(reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                switch(reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = (string)reader.Value;
                        if (!reader.Read()) throw new InvalidOperationException();
                        object value;
                        switch(reader.TokenType)
                        {
                            case JsonToken.Integer:
                            case JsonToken.String:
                                value = reader.Value;
                                break;
                            case JsonToken.StartObject:
                                switch(memberName)
                                {
                                    case "items":
                                    case "values":
                                    case "type":
                                        value = AvroFieldTypeSerializer.ReadFieldType(reader, serializer); break;
                                    default: throw new InvalidOperationException("Unexpected data: " + reader.Path);
                                }
                                break;
                            case JsonToken.StartArray:
                                switch(memberName)
                                {
                                    case "type": value = AvroFieldTypeSerializer.ReadFieldType(reader, serializer); break;
                                    case "fields": value = serializer.Deserialize<List<AvroField>>(reader); break;
                                    case "symbols": value = serializer.Deserialize<List<string>>(reader); break;
                                    default: throw new InvalidOperationException("Unexpected data: " + reader.Path);
                                }
                                break;
                            default:
                                throw new InvalidOperationException("Unexpected data: " + reader.Path);
                        }
                        fields.Add(memberName, value);
                        break;
                    default:
                        throw new InvalidOperationException("Unexpected: " + reader.TokenType.ToString());
                }
            }
            return AvroType.Create(fields);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanWrite { get { return false; } }
    }
    internal class AvroFieldTypeSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
        public static object ReadFieldType(JsonReader reader, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    return (string)reader.Value;
                case JsonToken.StartObject:
                    return serializer.Deserialize<AvroType>(reader);
                case JsonToken.StartArray:
                    reader.Read();
                    List<object> items = new List<object>();
                    do
                    {
                        items.Add(ReadFieldType(reader, serializer));
                    } while (reader.Read() && reader.TokenType != JsonToken.EndArray);
                    return items;
                default:
                    throw new NotImplementedException(reader.Path + ": " + reader.TokenType.ToString());
            }
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReadFieldType(reader, serializer);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override bool CanWrite { get { return false; } }
    }
}
