using System;
using System.Collections.Generic;
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

        private static readonly char[] Empty = new char[0];
        public virtual void Reset() { }
        internal virtual char[] GetCharBuffer(int charCount, out int charOffset)
        {
            if (charCount == 0)
            {
                charOffset = 0;
                return Empty;
            }
            if (charCount > CharsPerPage) throw new ArgumentOutOfRangeException("charCount");
            charOffset = 0;
            return new char[charCount];
        }

        internal static readonly Encoding Encoding = Encoding.UTF8;
        internal static readonly int MaxBytesPerCharacter = Encoding.GetMaxByteCount(1);
        internal const int CharsPerPage = 1024;
    }

    class PoolingAvroContext : AvroContext
    { // object re-use; not present unless a custom context is used

        private readonly List<char[]> pages = new List<char[]>();
        private int page = -1, charsLeftOnPage = 0, charIndex = 0;
        public override void Reset()
        {
            lock(pages)
            {
                if (pages.Count != 0)
                {
                    page = 0;
                    charsLeftOnPage = CharsPerPage;
                    charIndex = 0;
                }
            }
        }
        
        internal override char[] GetCharBuffer(int charCount, out int charOffset)
        {
            if (charCount == 0) return base.GetCharBuffer(charCount, out charOffset);
            lock(pages)
            {
                if(charCount <= charsLeftOnPage)
                {
                    charOffset = charIndex;
                    charIndex += charCount;
                    charsLeftOnPage -= charCount;
                    return pages[page];
                }
                if (++page == pages.Count)
                {
                    // need a new buffer
                    pages.Add(base.GetCharBuffer(CharsPerPage, out charOffset));
                }
                charOffset = 0;
                charIndex = charCount;
                charsLeftOnPage = CharsPerPage - charCount;
                return pages[page];
            }
        }

        public override string ToString()
        {
            lock(pages)
            {
                return string.Format("{0} pages allocated", pages.Count);
            }
        }
    }
}
