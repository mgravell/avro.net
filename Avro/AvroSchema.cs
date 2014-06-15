using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;

namespace Avro
{
    [JsonConverter(typeof(AvroTypeSerializer))]
    public abstract class AvroType {
        private static readonly JsonSerializer serializer = new JsonSerializer {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
       
        internal AvroType(Dictionary<string, object> fields)
        {
        }
            
        public static AvroType Parse(string text)
        {
            using (var sr = new StringReader(text))
            {
                return (AvroType)serializer.Deserialize(sr, typeof(AvroType));
            }
        }

        internal static AvroType Create(Dictionary<string, object> fields)
        {
            object type;
            if (!fields.TryGetValue("type", out type)) throw new InvalidOperationException("Expected: type");
            switch((string)type)
            {
                case "record": return new AvroRecord(fields);
                case "enum": return new AvroEnum(fields);
                case "fixed": return new AvroFixed(fields);
                case "map": return new AvroMap(fields);
                case "array": return new AvroArray(fields);
                default:
                    throw new InvalidOperationException("Unexpected type: " + type);
            }
        }

        public string GetSchema()
        {
            using(var sw = new StringWriter())
            {
                serializer.Serialize(sw, this);
                return sw.ToString();
            }
        }
    }

    public sealed class AvroRecord : AvroNamedType
    {
        public string doc { get; set; }
        private readonly List<AvroField> _fields;
        
        internal AvroRecord(Dictionary<string, object> fields) : base(fields)
        {
            object tmp;
            if (fields.TryGetValue("doc", out tmp)) doc = (string)tmp;
            if (fields.TryGetValue("fields", out tmp)) _fields = (List<AvroField>)tmp;

            if(_fields == null) _fields = new List<AvroField>();
        }
        public List<AvroField> fields { get { return _fields; } }

        
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortOrder
    {
        ascending,
        descending,
        ignore
    }
    
    public class AvroField
    {
        public string name { get; set; }
        public string doc { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(AvroFieldTypeSerializer))]
        public object type { get; set; }
        public object @default { get; set; }
        public SortOrder order { get; set; }

        private readonly List<string> _aliases = new List<string>();
        public List<string> aliases { get { return _aliases; } }

        public bool ShouldSerializealiases() {
            return _aliases.Count != 0;
        }
    }
    public abstract class AvroNamedType : AvroType {
        public string name { get; set; }
        public string @namespace { get; set; }
        private readonly List<string> _aliases;
        
        internal AvroNamedType(Dictionary<string, object> fields) : base(fields)
        {
            object tmp;
            if (fields.TryGetValue("name", out tmp)) name = (string)tmp;
            if (fields.TryGetValue("namespace", out tmp)) @namespace = (string)tmp;
            if (fields.TryGetValue("aliases", out tmp)) _aliases = (List<string>)tmp;
            if (_aliases == null) _aliases = new List<string>();
        }
        public List<string> aliases { get { return _aliases; } }

        public bool ShouldSerializealiases()
        {
            return _aliases.Count != 0;
        }
        
    }
    public class AvroEnum : AvroNamedType
    {
        public string doc { get; set; }
        private readonly List<string> _symbols;
        public List<string> symbols { get { return _symbols; } }

        internal AvroEnum(Dictionary<string, object> fields) : base(fields)
        {
            object tmp;
            if (fields.TryGetValue("doc", out tmp)) doc = (string)tmp;
            if (fields.TryGetValue("symbols", out tmp)) _symbols = (List<string>)tmp;

            if(_symbols == null) _symbols = new List<string>();
        }
    }
    public class AvroFixed : AvroNamedType
    {
        internal AvroFixed(Dictionary<string, object> fields) : base(fields)
        {
            object tmp;
            if (fields.TryGetValue("size", out tmp))
            {
                if (tmp is long) size = (int)(long)tmp;
                else if (tmp is int) size = (int)tmp;
                else size = Convert.ToInt32(tmp);
            }
        }
        public int size { get; set; }
    }

    public class AvroArray : AvroType
    {
        public object items { get; set; }
        internal AvroArray(Dictionary<string, object> fields) : base(fields)
        {
            object tmp;
            if (fields.TryGetValue("items", out tmp)) items = tmp;
        }
    }
    public class AvroMap : AvroType
    {
        public object values { get; set; }

        internal AvroMap(Dictionary<string, object> fields) : base(fields)
        {
            object tmp;
            if (fields.TryGetValue("values", out tmp)) values = tmp;
        }
    }
}
