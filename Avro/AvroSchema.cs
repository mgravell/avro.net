using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Avro
{
    public class AvroSchema
    {
        public string @namespace { get; set; }
        public string doc { get; set; }
        private readonly List<string> _aliases = new List<string>();
        public List<string> aliases { get { return _aliases; } }
        public RecordType type { get; set; }
        public string name { get; set; }
        private readonly List<AvroField> _fields = new List<AvroField>();
        public List<AvroField> fields { get { return _fields; } }

        static JsonSerializer serializer;
        private static JsonSerializer GetSerializer()
        {
            return serializer ?? (
                serializer = new JsonSerializer {
                    // Converters = { new AvroFieldConverter() }
                });
        }
        public static AvroSchema Parse(string text)
        {
            var ser = GetSerializer();
            using (var sr = new StringReader(text)) {
                return (AvroSchema)ser.Deserialize(sr, typeof(AvroSchema));
            }
        }
    }

    public enum RecordType
    {
        record,
        @enum,
        array,
        map,
        @fixed
    }
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
        public string @default { get; set; }
        public SortOrder order { get; set; }

        private readonly List<string> _aliases = new List<string>();
        public List<string> aliases { get { return _aliases; } }
    }
    public class AvroEnum
    {
        public string name { get; set; }
        public string @namespace { get; set; }
        public string doc { get; set; }
        private readonly List<string> _aliases = new List<string>();
        public List<string> aliases { get { return _aliases; } }
        private readonly List<string> _symbols = new List<string>();
        public List<string> symbols { get { return _symbols; } }
    }
    public class AvroFixed
    {
        public string name { get; set; }
        public string @namespace { get; set; }
        private readonly List<string> _aliases = new List<string>();
        public List<string> aliases { get { return _aliases; } }
        public int size { get; set; }
    }

    public class AvroArray
    {
        public string items { get; set; }
    }
    public class AvroMap
    {
        public string values { get; set; }
    }
}
