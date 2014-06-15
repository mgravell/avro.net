using System;
using System.IO;

namespace Avro
{
    public class AvroSerializer
    {
        private readonly Type baseType;
        public AvroSerializer(Type baseType)
        {
            if (baseType == null) throw new ArgumentNullException();
            this.baseType = baseType;
        }

        public void Serialize(Stream destination, object obj, AvroContext ctx = null)
        {
            throw new NotImplementedException();
        }
        public object Deserialize(Stream source, AvroContext ctx = null)
        {
            throw new NotImplementedException();
        }
    }
    public sealed  class AvroSerializer<T> : AvroSerializer, IAvroSerializer<T>
    {
        public AvroSerializer() : base(typeof(T)) { }

        public void Serialize(Stream destination, T obj, AvroContext ctx = null)
        {
            base.Serialize(destination, obj);
        }
        public new T Deserialize(Stream source, AvroContext ctx = null)
        {
            return (T)base.Deserialize(source);
        }
    }

    public interface IAvroSerializer<T>
    {
        void Serialize(Stream destination, T obj, AvroContext ctx = null);
        T Deserialize(Stream source, AvroContext ctx = null);
    }
}
