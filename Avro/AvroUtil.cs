using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avro
{
    internal class AvroUtil
    {
        public static uint ZigZag(int value)
        {
            return (uint)((value << 1) ^ (value >> 31));
        }
        public static int UnZigZag(uint value)
        {
            const int MSB = ((int)1) << 31;
            int tmp = (int)value;
            return (-(tmp & 0x01)) ^ ((tmp >> 1) & ~MSB);
        }
        public static ulong ZigZag(long value)
        {
            return (ulong)((value << 1) ^ (value >> 63));
        }
        public static long UnZigZag(ulong value)
        {
            const long MSB = ((long)1) << 63;
            long tmp = (long)value;
            return (-(tmp & 0x01L)) ^ ((tmp >> 1) & ~MSB);
        }
    }
}
