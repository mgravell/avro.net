using System.Text;

namespace Avro
{
    public class AvroString
    {
        private readonly string _value;
        private readonly int _length;
        private readonly unsafe char* _valuePtr;
        private readonly int _lenght;

        public AvroString(string value)
        {
            _value = value;
            _lenght = value.Length;
        }
        
        public unsafe AvroString(char* value, int length)
        {
            _valuePtr = value;
            _length = length;
        }

        public static implicit operator AvroString(string value)
        {
            return value == null ? null : new AvroString(value);
        }

        public void Write(byte[] bufferIo, Encoding encoding)
        {
            if (_value != null)
            {
                encoding.GetBytes(_value, 0, _value.Length, bufferIo, 0);
            }
            else
                unsafe
                {
                    fixed (byte* b = bufferIo)
                    {
                        encoding.GetBytes(_valuePtr, _lenght, b, 0);
                    }
                }
        }

        public override unsafe string ToString()
        {
            if (_value != null)
            {
                return _value;
            }

            if (_valuePtr != default(char*))
            {
                return new string(_valuePtr,0,_lenght);
            }

            return "";
        }
        
        public int Length { get { return _lenght; } }
        
        public static bool operator ==(string x, AvroString y)
        {
            if ((object)x == null) return (object)y == null;
            if ((object)y == null || x.Length != y.Length) return false;

            return x == y.ToString();
        }
        public static bool operator !=(string x, AvroString y)
        {
            return !(x == y);
        }
        public static bool operator ==(AvroString x, string y)
        {
            if ((object)x == null) return (object)y == null;
            if ((object)y == null || x.Length != y.Length) return false;

            return y == x.ToString();
        }
        public static bool operator !=(AvroString x, string y)
        {
            return !(x == y);
        }
    }
}
