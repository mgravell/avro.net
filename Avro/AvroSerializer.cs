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

        public void Serialize(Stream destination, object obj)
        {
            throw new NotImplementedException();
        }
        public object Deserialize(Stream source)
        {
            throw new NotImplementedException();
        }
    }
    public sealed  class AvroSerializer<T> : AvroSerializer, IAvroSerializer<T>
    {
        public AvroSerializer() : base(typeof(T)) { }

        public void Serialize(Stream destination, T obj)
        {
            base.Serialize(destination, obj);
        }
        public new T Deserialize(Stream source)
        {
            return (T)base.Deserialize(source);
        }
    }

    public interface IAvroSerializer<T>
    {
        void Serialize(Stream destination, T obj);
        T Deserialize(Stream source);
    }
}
