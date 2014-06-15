﻿using System;
using System.IO;
using System.Text;

namespace Avro
{
    public abstract class AvroWriter : IDisposable
    {
        public static AvroWriter Create(Stream destination)
        {
            if (BitConverter.IsLittleEndian) return new LittleEndianAvroWriter(destination);
            throw new NotImplementedException("big endian atchitecture");
        }
        public abstract void WriteSingle(float value);
        public abstract void WriteDouble(double value);
        internal AvroWriter(Stream destination)
        {
            if (destination == null) throw new ArgumentNullException();
            this.dest = destination;

            const int BUFFER_SIZE = 1024;
            ioBuffer = new byte[BUFFER_SIZE];
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

        static readonly Encoding encoding = Encoding.UTF8;
        public void WriteString(string value)
        {
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
        public LittleEndianAvroWriter(Stream destination) : base(destination) { }
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
