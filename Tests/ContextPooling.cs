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
        [Test]
        public void MultipleDeserialize_FewAllocations()
        {

            using (var ctx = AvroContext.Create())
            {
                var obj = new CheapStrings("abc"); //<=== note that this is just me being lazy to investigate;
                                                   // the real impl would expose reader/writer APIs to manipulate
                                                   // the string, presumably
                var ser = new CheapStringsSerializer();
                using (var ms = new MemoryStream())
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        ser.Serialize(ms, obj, ctx);
                    }
                    ms.Position = 0;
                    for (int i = 0; i < 1000; i++)
                    {
                        var clone = ser.Deserialize(ms, ctx);
                        bool equiv = clone.Foo == "abc"; // convenience equality test operator
                        Assert.IsTrue(equiv);
                        ctx.Reset();
                    }
                }

                Assert.AreEqual("1 pages allocated", ctx.ToString());
            }
        }
    }

    public class CheapStrings
    {
        private readonly AvroString foo;
        public AvroString Foo { get { return foo; } }
        public CheapStrings(AvroString foo)
        {
            this.foo = foo;
        }
    }
    // what we want to generate, ish...
    public class CheapStringsSerializer : IAvroSerializer<CheapStrings>
    {
        public void Serialize(Stream destination, CheapStrings obj, AvroContext ctx = null)
        {
            using (var writer = AvroWriter.Create(destination, ctx))
            {
                writer.WriteAvroString(obj.Foo);
            }
        }
        public CheapStrings Deserialize(Stream source, AvroContext ctx = null)
        {
            using (var reader = AvroReader.Create(source, ctx))
            {
                AvroString foo = reader.ReadAvroString();
                CheapStrings obj = new CheapStrings(
                    foo: foo
                );
                return obj;
            }
        }
    }
}
