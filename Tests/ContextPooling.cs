using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avro;
using NUnit.Framework;
using System.IO;

namespace Tests
{

    /// <summary>
    /// Idea to explore here is : how can we efficiently process large numbers of objects
    /// </summary>
    [TestFixture]
    public class ContextPooling
    {
        private readonly Stopwatch sw;

        public ContextPooling()
        {
            sw = Stopwatch.StartNew();
        }

        public IEnumerable<TestCaseData> GetContexts()
        {
            Func<Stream, AvroContext, object> cheapDeserialize = new CheapStringsSerializer().Deserialize;
            Func<Stream, AvroContext, object> notSoCheapDeserialize = new NotSoCheapStringsSerializer().Deserialize;
            yield return new TestCaseData(AvroContext.Create(), cheapDeserialize).SetName("Pooling");
            yield return new TestCaseData(AvroContext.Default, notSoCheapDeserialize).SetName("Default");
        }

        [TestCaseSource("GetContexts")]
        public unsafe void MultipleDeserialize_SpeedTestWithGC(AvroContext ctx, Func<Stream, AvroContext, object> deserialize)
        {
            const int testSize = 1 * 1000 * 1000;

            const string str = "abcdefghijklmnoprstuwxyz";
            var handle = GCHandle.Alloc(str, GCHandleType.Pinned);
            handle.AddrOfPinnedObject();

            try
            {
                var avroString = new AvroString((char*)handle.AddrOfPinnedObject(), str.Length);
                var obj = new CheapStrings(avroString, avroString);
                var ser = new CheapStringsSerializer();
                using (var ms = new MemoryStream())
                {
                    for (var i = 0; i < testSize; i++)
                    {
                        ser.Serialize(ms, obj, ctx);
                    }

                    ms.Position = 0;
                    sw.Restart();
                    for (var i = 0; i < testSize; i++)
                    {
                        var clone = deserialize(ms, ctx);
                        ctx.Reset();
                    }

                    GC.Collect(2, GCCollectionMode.Forced );

                    sw.Stop();
                    Console.WriteLine("Test with GC took: {0}", sw.Elapsed);
                }
            }
            finally
            {
                handle.Free();
            }

            //Assert.AreEqual("1 pages allocated", ctx.ToString());
        }

        [Test]
        public void MultipleDeserialize_FewAllocations()
        {
            using (var ctx = AvroContext.Create())
            {
                const string str = "abc";
                var handle = GCHandle.Alloc(str, GCHandleType.Pinned);
                handle.AddrOfPinnedObject();

                try
                {
                    var obj = new NotSoCheapStrings(str, str);
                    var ser = new NotSoCheapStringsSerializer();

                    var cheapSerializer = new CheapStringsSerializer();
                    using (var ms = new MemoryStream(10 * 1024 *1024))
                    {
                        for (var i = 0; i < 1000; i++)
                        {
                            ser.Serialize(ms, obj, ctx);
                        }
                        ms.Position = 0;
                        for (int i = 0; i < 1000; i++)
                        {
                            var clone = cheapSerializer.Deserialize(ms, ctx);
                            bool equiv = clone.Foo == "abc"; // convenience equality test operator
                            Assert.IsTrue(equiv);
                            ctx.Reset();
                        }
                    }
                }
                finally
                {
                    handle.Free();
                }
            }
        }
    }

    public class CheapStrings
    {
        private readonly AvroString bar;
        private readonly AvroString foo;
        public AvroString Foo { get { return foo; } }
        public AvroString Bar { get { return bar; } }

        public CheapStrings(AvroString bar, AvroString foo)
        {
            this.bar = bar;
            this.foo = foo;
        }
    }

    public class CheapStringsSerializer : IAvroSerializer<CheapStrings>
    {
        public void Serialize(Stream destination, CheapStrings obj, AvroContext ctx = null)
        {
            using (var writer = AvroWriter.Create(destination, ctx))
            {
                writer.WriteAvroString(obj.Foo);
                writer.WriteAvroString(obj.Bar);
            }
        }

        public CheapStrings Deserialize(Stream source, AvroContext ctx = null)
        {
            using (var reader = AvroReader.Create(source, ctx))
            {
                var foo = reader.ReadAvroString();
                var bar = reader.ReadAvroString();
                
                return new CheapStrings(
                    foo: foo,
                    bar: bar
                    );
            }
        }
    }

    public class NotSoCheapStrings
    {
        private readonly string bar;
        private readonly string foo;
        public string Foo { get { return foo; } }
        public string Bar { get { return bar; } }

        public NotSoCheapStrings(string bar, string foo)
        {
            this.bar = bar;
            this.foo = foo;
        }
    }

    public class NotSoCheapStringsSerializer : IAvroSerializer<NotSoCheapStrings>
    {
        public void Serialize(Stream destination, NotSoCheapStrings obj, AvroContext ctx = null)
        {
            using (var writer = AvroWriter.Create(destination, ctx))
            {
                writer.WriteString(obj.Foo);
                writer.WriteString(obj.Bar);
            }
        }

        public NotSoCheapStrings Deserialize(Stream source, AvroContext ctx = null)
        {
            using (var reader = AvroReader.Create(source, ctx))
            {
                var foo = reader.ReadString();
                var bar = reader.ReadString();

                return new NotSoCheapStrings(
                    foo: foo,
                    bar: bar);
            }
        }
    }
}
