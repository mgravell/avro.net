using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Avro
{
    public class AvroContext : IDisposable
    {
        public static AvroContext Default = new AvroContext();

        public static AvroContext Create()
        {
            return new PoolingAvroContext();
        }

        internal AvroContext() { }

        public virtual void Dispose() { }

        public virtual void Reset() { }

        internal static readonly Encoding Encoding = Encoding.UTF8;

        // internal static readonly int MaxBytesPerCharacter = Encoding.GetMaxByteCount(1);
        internal const int MaxBytesPerCharacter = 4; // since rfc3629 (2003), we don't need to worry about the 5/6 byte ranges

        internal virtual AvroString GetAvroString(byte[] ioBuffer, int byteCount)
        {
            return new AvroString(Encoding.GetString(ioBuffer, 0, byteCount));
        }

        internal void WriteAvroString(byte[] ioBuffer, AvroString str)
        {
            str.Write(ioBuffer, Encoding);
        }
    }

    class PoolingAvroContext : AvroContext
    {
        private const int AllocSize = 1024 * 1024;
        private const int BytesPerChar = 2;
        
        private readonly IntPtr ptr;
        private readonly IntPtr upTo;
        private IntPtr current;

        internal PoolingAvroContext()
        {
            ptr = Marshal.AllocHGlobal(AllocSize);
            upTo = ptr + AllocSize;
            current = ptr;
        }

        public override void Dispose()
        {
            Marshal.FreeHGlobal(ptr);
        }

        public override void Reset()
        {
            current = ptr;
        }

        internal override unsafe AvroString GetAvroString(byte[] ioBuffer, int byteCount)
        {
            var charsLeft = (int)((upTo.ToInt64() - current.ToInt64()) / BytesPerChar);

            fixed (byte* b = ioBuffer)
            {
                var start = (char*)current;
                var charsRead = Encoding.GetChars(b, byteCount, start, charsLeft);
                current += (charsRead * BytesPerChar);
                return new AvroString(start, charsRead);
            }
        }
    }
}
