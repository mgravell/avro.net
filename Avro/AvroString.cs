using System;
using System.Collections.Generic;
using System.Text;

namespace Avro
{
    public class AvroString
    {
        public AvroString() { }
        private AvroString(string value)
        {
            Append(value);
        }
        private readonly List<ArraySegment<char>> segments = new List<ArraySegment<char>>();
        private int length;
        internal List<ArraySegment<char>> Segments { get { return segments; } }
        public void Append(string value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (value.Length == 0) return;
            var c = value.ToCharArray();
            segments.Add(new ArraySegment<char>(c, 0, c.Length));
            length += c.Length;
        }
        public void Append(char[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (value.Length == 0) return;
            segments.Add(new ArraySegment<char>(value, 0, value.Length));
            length += value.Length;
        }
        public void Append(char[] value, int offset, int count)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (count == 0) return;
            segments.Add(new ArraySegment<char>(value, offset, count));
            length += count;
        }

        public static implicit operator AvroString(string value)
        {
            return value == null ? null : new AvroString(value);
        }

        public override string ToString()
        {
            var segments = this.segments;
            switch (segments.Count)
            {
                case 0: return "";
                case 1:
                    {
                        var segment = segments[0];
                        return new string(segment.Array, segment.Offset, segment.Count);
                    }
                default:
                    {
                        var sb = new StringBuilder();
                        foreach (var segment in segments)
                        {
                            sb.Append(segment.Array, segment.Offset, segment.Count);
                        }
                        return sb.ToString();
                    }
            }
        }
        public int Length { get { return Length; } }
        
        public static bool operator ==(string x, AvroString y)
        {
            if ((object)x == null) return (object)y == null;
            if ((object)y == null || x.Length != y.length) return false;

            // not optimized!
            int charIndex = 0;
            foreach (var segment in y.segments)
            {
                char[] arr = segment.Array;
                int max = segment.Offset + segment.Count;
                for (int i = segment.Offset; i < max; i++)
                {
                    if (x[charIndex++] != arr[i]) return false;
                }
            }
            return true;
        }
        public static bool operator !=(string x, AvroString y)
        {
            return !(x == y);
        }
        public static bool operator ==(AvroString x, string y)
        {
            if ((object)y == null) return (object)x == null;
            if ((object)x == null || y.Length != x.length) return false;

            // not optimized!
            int charIndex = 0;
            foreach (var segment in x.segments)
            {
                char[] arr = segment.Array;
                int max = segment.Offset + segment.Count;
                for (int i = segment.Offset; i < max; i++)
                {
                    if (y[charIndex++] != arr[i]) return false;
                }
            }
            return true;
        }
        public static bool operator !=(AvroString x, string y)
        {
            return !(x == y);
        }
    }
}
