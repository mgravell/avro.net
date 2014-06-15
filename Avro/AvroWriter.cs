using System;
using System.IO;
using System.Text;

namespace Avro
{
    public abstract class AvroWriter : IDisposable
    {
        public static AvroWriter Create(Stream destination, AvroContext ctx = null)
        {
            if (BitConverter.IsLittleEndian) return new LittleEndianAvroWriter(destination, ctx);
            throw new NotImplementedException("big endian atchitecture");
        }
        private readonly AvroContext context;
        public abstract void WriteSingle(float value);
        public abstract void WriteDouble(double value);
        internal AvroWriter(Stream destination, AvroContext ctx)
        {
            if (destination == null) throw new ArgumentNullException();
            this.dest = destination;
            this.context = ctx ?? AvroContext.Default;

            const int BUFFER_SIZE = 1024;
            ioBuffer = new byte[BUFFER_SIZE];  // this is just basic now; would be pooled etc
            ioRemaining = BUFFER_SIZE;
        }
        public void WriteInt64(int value)
        {
            WriteUInt64(AvroUtil.ZigZag(value));
        }
        public void WriteUInt64(uint value)
        {
            throw new NotImplementedException();
        }

        public void WriteAvroString(AvroString foo)
        {
            var segments = foo.Segments;

            int byteCount = 0;
            // could probably support >2GB strings here, if we want

            var encoding = AvroContext.Encoding;

            switch(segments.Count)
            {
                case 0:
                    WriteUInt32(AvroUtil.ZigZag(0));
                    break;
                case 1:
                    {
                        var segment0 = segments[0];
                        int byteCount0 = encoding.GetByteCount(segment0.Array, segment0.Offset, segment0.Count);
                        WriteUInt32(AvroUtil.ZigZag(byteCount0));
                        int offset = Reserve(byteCount0);
                        encoding.GetBytes(segment0.Array, segment0.Offset, segment0.Count, ioBuffer, offset);
                    }
                    break;
                case 2:
                    {
                        var segment0 = segments[0];
                        var segment1 = segments[1];
                        int byteCount0 = encoding.GetByteCount(segment0.Array, segment0.Offset, segment0.Count);
                        int byteCount1 = encoding.GetByteCount(segment1.Array, segment1.Offset, segment1.Count);
                        WriteUInt32(AvroUtil.ZigZag(byteCount0 + byteCount1));
                        int offset = Reserve(byteCount0);
                        encoding.GetBytes(segment0.Array, segment0.Offset, segment0.Count, ioBuffer, offset);
                        offset = Reserve(byteCount1);
                        encoding.GetBytes(segment1.Array, segment1.Offset, segment1.Count, ioBuffer, offset);
                    }
                    break;
                case 3:
                    {
                        var segment0 = segments[0];
                        var segment1 = segments[1];
                        var segment2 = segments[2];
                        int byteCount0 = encoding.GetByteCount(segment0.Array, segment0.Offset, segment0.Count);
                        int byteCount1 = encoding.GetByteCount(segment1.Array, segment1.Offset, segment1.Count);
                        int byteCount2 = encoding.GetByteCount(segment2.Array, segment2.Offset, segment2.Count);
                        WriteUInt32(AvroUtil.ZigZag(byteCount0 + byteCount1 + byteCount2));
                        int offset = Reserve(byteCount0);
                        encoding.GetBytes(segment0.Array, segment0.Offset, segment0.Count, ioBuffer, offset);
                        offset = Reserve(byteCount1);
                        encoding.GetBytes(segment1.Array, segment1.Offset, segment1.Count, ioBuffer, offset);
                        offset = Reserve(byteCount2);
                        encoding.GetBytes(segment2.Array, segment2.Offset, segment2.Count, ioBuffer, offset);
                    }
                    break;
                default:
                    {
                        foreach (var segment in segments)
                        {
                            byteCount += encoding.GetByteCount(segment.Array, segment.Offset, segment.Count);
                        }
                        WriteUInt32(AvroUtil.ZigZag(byteCount));
                        int perChar = AvroContext.MaxBytesPerCharacter;
                        foreach (var segment in foo.Segments)
                        {
                            int reserved = perChar * segment.Count;
                            int offset = Reserve(reserved);
                            // but we were almost certainly wrong (over-estimate)...
                            int delta = reserved - encoding.GetBytes(segment.Array, segment.Offset, segment.Count, ioBuffer, offset);
                            ioRemaining += delta;
                            ioOffset -= delta;
                        }
                    }
                    break;
            }
        }

        public void WriteString(string value)
        {
            var encoding = AvroContext.Encoding;
            int byteCount = encoding.GetByteCount(value);
            WriteUInt32(AvroUtil.ZigZag(byteCount));
            int offset = Reserve(byteCount);
            encoding.GetBytes(value, 0, value.Length, ioBuffer, offset);
        }
        private readonly Stream dest;
        protected byte[] ioBuffer;
        private int ioOffset, ioRemaining;

        public void Dispose() { Flush(); }

        public void WriteBoolean(bool value)
        {
            ioBuffer[Reserve(1)] = value ? (byte)1 : (byte)0;
        }
        public void WriteInt32(int value)
        {
            WriteUInt32(AvroUtil.ZigZag(value));
        }
        public void WriteUInt32(uint value)
        {
            if ((value & 0x7F) == 0) // 1 byte
            {
                ioBuffer[Reserve(1)] = (byte)value;
            }
            else if ((value & 0x3FFF) == 0) // 2 bytes
            {
                int offset = Reserve(2);
                ioBuffer[offset++] = (byte)((value & 0x7F) | 0x80);
                ioBuffer[offset] = (byte)(value >> 7);
            }
            else if ((value & 0x1FFFFF) == 0) // 3 bytes
            {
                int offset = Reserve(2);
                ioBuffer[offset++] = (byte)((value & 0x7F) | 0x80);
                ioBuffer[offset++] = (byte)(((value >> 7) & 0x7F) | 0x80);
                ioBuffer[offset] = (byte)(value >> 14);
            }
            else if ((value & 0xFFFFFFF) == 0) // 4 bytes
            {
                int offset = Reserve(4);
                ioBuffer[offset++] = (byte)((value & 0x7F) | 0x80);
                ioBuffer[offset++] = (byte)(((value >> 7) & 0x7F) | 0x80);
                ioBuffer[offset++] = (byte)(((value >> 14) & 0x7F) | 0x80);
                ioBuffer[offset] = (byte)(value >> 21);
            }
            else // 5 bytes
            {
                int offset = Reserve(5);
                ioBuffer[offset++] = (byte)((value & 0x7F) | 0x80);
                ioBuffer[offset++] = (byte)(((value >> 7) & 0x7F) | 0x80);
                ioBuffer[offset++] = (byte)(((value >> 14) & 0x7F) | 0x80);
                ioBuffer[offset++] = (byte)(((value >> 21) & 0x7F) | 0x80);
                ioBuffer[offset] = (byte)(value >> 28);
            }
        }
        public void Flush()
        {
            if (ioOffset != 0) dest.Write(ioBuffer, 0, ioOffset);
            ioOffset = 0; ioRemaining = ioBuffer.Length;
        }
        protected int Reserve(int count)
        {
            if (ioRemaining < count)
            {
                if (ioOffset != 0) Flush();
                if (ioRemaining < count)
                {
                    throw new InvalidOperationException("Proof of concept is assuming all data fits the fixed-size buffer");
                }
            }
            int tmp = ioOffset;
            ioOffset += count;
            ioRemaining -= count;
            return tmp;
        }
    }
    internal sealed class LittleEndianAvroWriter : AvroWriter
    {
        public LittleEndianAvroWriter(Stream destination, AvroContext ctx) : base(destination, ctx) { }
        public override unsafe void WriteSingle(float value)
        {
            int offset = Reserve(4);
            fixed(void* ptr = &ioBuffer[offset])
            {
                *((float*)ptr) = value;
            }
        }
        public override unsafe void WriteDouble(double value)
        {
            int offset = Reserve(8);
            fixed (void* ptr = &ioBuffer[offset])
            {
                *((double*)ptr) = value;
            }
        }
    }
}
