using System;
using System.IO;

namespace Avro
{
    public abstract class AvroReader : IDisposable
    {
        private readonly Stream source;
        protected byte[] ioBuffer = new byte[1024]; // this is just basic now; would be pooled etc
        private readonly AvroContext context;

        public abstract float ReadSingle();
        public abstract double ReadDouble();
        public static AvroReader Create(Stream source, AvroContext ctx = null)
        {
            if (BitConverter.IsLittleEndian) return new LittleEndianAvroReader(source, ctx);
            throw new NotImplementedException("big endian atchitecture");
        }
        internal AvroReader(Stream source, AvroContext ctx)
        {
            if (source == null) throw new ArgumentNullException();
            this.source = source;
            this.context = ctx ?? AvroContext.Default;
        }
        public void Dispose() { }

        public string ReadString()
        {
            int byteCount = ReadInt32();
            Read(byteCount);
            return AvroContext.Encoding.GetString(ioBuffer, 0, byteCount);
        }

        public bool ReadBoolean()
        {
            int i = source.ReadByte();
            switch (i)
            {
                case 0:
                    return false;
                case 1:
                    return true;
                default:
                    if (i < 0) throw new EndOfStreamException();
                    throw new InvalidOperationException(string.Format("Invalid boolean value (expected 0/1, got {0})", i));
            }
        }
        
        public AvroString ReadAvroString()
        {
            var byteCount = ReadInt32();
            Read(byteCount);

            return context.GetAvroString( ioBuffer, byteCount);
        }

        public int ReadInt32()
        {
            return AvroUtil.UnZigZag(ReadUInt32());
        }
        public uint ReadUInt32()
        {
            uint value = 0;
            int b = source.ReadByte();
            if (b < 0) throw new EndOfStreamException();
            value = (uint)b;
            if ((value & 0x80) == 0) return value;
            value &= 0x7F;

            b = source.ReadByte();
            if (b < 0) throw new EndOfStreamException();
            value |= ((uint)b & 0x7F) << 7;
            if ((b & 0x80) == 0) return value;

            b = source.ReadByte();
            if (b < 0) throw new EndOfStreamException();
            value |= ((uint)b & 0x7F) << 14;
            if ((b & 0x80) == 0) return value;

            b = source.ReadByte();
            if (b < 0) throw new EndOfStreamException();
            value |= ((uint)b & 0x7F) << 21;
            if ((b & 0x80) == 0) return value;

            b = source.ReadByte();
            if (b < 0) throw new EndOfStreamException();
            value |= (uint)b << 28; // can only use 4 bits from this chunk
            if ((b & 0xF0) == 0) return value;

            throw new OverflowException();
        }
        protected void Read(int count)
        {
            if (count > ioBuffer.Length) throw new InvalidOperationException("Proof of concept is assuming all data fits the fixed-size buffer");
            int offset = 0, bytesRead;
            while (count > 0 && (bytesRead = source.Read(ioBuffer, offset, count)) > 0)
            {
                offset += bytesRead;
                count -= bytesRead;
            }
            if (count != 0) throw new EndOfStreamException();
        }
    }
    internal sealed class LittleEndianAvroReader : AvroReader
    {
        public LittleEndianAvroReader(Stream source, AvroContext ctx) : base(source, ctx) { }
        public override unsafe float ReadSingle()
        {
            Read(4);
            fixed (void* ptr = ioBuffer)
            {
                return *((float*)ptr);
            }
        }
        public override unsafe double ReadDouble()
        {
            Read(8);
            fixed (void* ptr = ioBuffer)
            {
                return *((double*)ptr);
            }
        }
    }
}
