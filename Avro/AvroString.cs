using System.Text;

namespace Avro
{
    public struct AvroString
    {
        public readonly unsafe char* ValuePtr;
        public readonly int Length;

        public unsafe AvroString(char* value, int length)
        {
            ValuePtr = value;
            Length = length;
        }

        public void Write(byte[] bufferIo, Encoding encoding)
        {
            unsafe
            {
                fixed (byte* b = bufferIo)
                {
                    encoding.GetBytes(ValuePtr, Length, b, bufferIo.Length);
                }
            }
        }

        public override unsafe string ToString()
        {
            if (ValuePtr != default(char*))
            {
                return new string(ValuePtr, 0, Length);
            }

            return "";
        }

        public unsafe bool Equals(AvroString other)
        {
            var otherLength = other.Length;
            var otherPtr = other.ValuePtr;

            return Equals(otherLength, otherPtr);
        }

        public unsafe bool Equals(int otherLength, char* otherPtr)
        {
            if (Length != otherLength)
                return false;

            var length = Length;

            var ch1 = ValuePtr;
            var ch2 = otherPtr;
            const int delta = 8;

            while (length >= delta)
            {
                if (*(long*) ch1 != *(long*) ch2)
                    return false;

                ch1 += delta;
                ch2 += delta;
                length -= delta;
            }

            while (length > 1 && *(int*) ch1 == *(int*) ch2)
            {
                ch1 += 2;
                ch2 += 2;
                length -= 2;
            }

            if (length == 1 && *ch1 == *ch2)
                return true;

            return length <= 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AvroString && Equals((AvroString)obj);
        }

        public static bool operator ==(AvroString left, AvroString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AvroString left, AvroString right)
        {
            return !left.Equals(right);
        } 
        
        public static unsafe bool operator ==(AvroString left, string right)
        {
            fixed (char* c = right)
            {
                return left.Equals(right.Length, c);    
            }
        }

        public static unsafe bool operator !=(AvroString left, string right)
        {
            fixed (char* c = right)
            {
                return left.Equals(right.Length, c) == false;
            }
        }

        public override int GetHashCode()
        {
            unsafe
            {
                unchecked
                {
                    return ((int)ValuePtr * 397) ^ Length;
                }
            }
        }
    }
}
