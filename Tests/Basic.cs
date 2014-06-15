using Avro;
using NUnit.Framework;
using System.IO;


/// <summary>
/// I have no idea at this point whether this is even writing the correct format; that isn't important for this test - this is mainly to
/// explore some of the types of models it should work for, for the code-first / hybrid scenarios, and to illustrate
/// the type of code/IL that would be emitted to do the actual grunt work
/// </summary>

namespace Tests
{
    [TestFixture]
    public class Basic
    {
        [Test]
        public void TestObviousWhatToDo()
        {
            var ser = new ObviousWhatToDoSerializer();
            ObviousWhatToDo orig = new ObviousWhatToDo(123, "abc", 456.78F), clone;
            using (var ms = new MemoryStream())
            {
                ser.Serialize(ms, orig);
                ms.Position = 0;
                clone = ser.Deserialize(ms);
                Assert.AreEqual(ms.Length, ms.Position);
            }            
            Assert.AreNotSame(orig, clone);
            Assert.AreEqual(123, clone.Foo);
            Assert.AreEqual("abc", clone.Bar);
            Assert.AreEqual(456.78F, clone.Blap);
        }

        [Test]
        public void TestAttributeBased()
        {
            var ser = new AttributeBasedSerializer();
            AttributeBased orig = new AttributeBased(123, "abc", 456.78F), clone;
            using (var ms = new MemoryStream())
            {
                ser.Serialize(ms, orig);
                ms.Position = 0;
                clone = ser.Deserialize(ms);
                Assert.AreEqual(ms.Length, ms.Position);
            }
            Assert.AreNotSame(orig, clone);
            Assert.AreEqual(123, clone.Foo);
            Assert.AreEqual("abc", clone.Bar);
            Assert.AreEqual(456.78F, clone.Blap);
        }
    }

    public class ObviousWhatToDo
    {
        private readonly int foo;
        private readonly string bar;
        private readonly float blap;
        public int Foo { get { return foo; } }
        public string Bar { get { return bar; } }
        public float Blap { get { return blap; } }
        public ObviousWhatToDo(int foo, string bar, float blap = 123.45F)
        {
            this.foo = foo;
            this.bar = bar;
            this.blap = blap;
        }
    }
    // what we want to generate, ish...
    public class ObviousWhatToDoSerializer : IAvroSerializer<ObviousWhatToDo>
    {
        public void Serialize(Stream destination, ObviousWhatToDo obj)
        {
            using (var writer = new AvroWriter(destination))
            {
                writer.WriteInt32(obj.Foo);
                writer.WriteString(obj.Bar);
                writer.WriteSingle(obj.Blap);
            }
        }
        public ObviousWhatToDo Deserialize(Stream source)
        {
            using (var reader = new AvroReader(source))
            {
                int foo = reader.ReadInt32();
                string bar = reader.ReadString();
                float blap = reader.ReadSingle();
                ObviousWhatToDo obj = new ObviousWhatToDo(
                    foo: foo,
                    bar: bar,
                    blap: blap
                );
                return obj;
            }
        }
    }


    [AvroContract]
    public class AttributeBased
    {
        private readonly int foo;
        private readonly string bar;
        private float blap;
        public int Foo { get { return foo; } }
        public string Bar { get { return bar; } }
        [AvroField(3)]
        public float Blap { get { return blap; } set { blap = value; } }

        [AvroConstructor] // optional; could also be specified on a static method that returns an instance
        public AttributeBased([AvroField(2)] int foo, [AvroField(1)] string bar, float blap = 123.45F)
        {
            this.foo = foo;
            this.bar = bar;
            this.blap = blap;
        }
    }

    public class AttributeBasedSerializer : IAvroSerializer<AttributeBased>
    {
        public void Serialize(Stream destination, AttributeBased obj)
        {
            using (var writer = new AvroWriter(destination))
            {
                writer.WriteString(obj.Bar);
                writer.WriteInt32(obj.Foo);                
                writer.WriteSingle(obj.Blap);
            }
        }
        public AttributeBased Deserialize(Stream source)
        {
            using (var reader = new AvroReader(source))
            {
                string bar = reader.ReadString();
                int foo = reader.ReadInt32();
                float blap = reader.ReadSingle();
                AttributeBased obj = new AttributeBased(
                    foo: foo,
                    bar: bar,
                    blap: 123.45F
                ); // note explicitly supply unusued defaults, to ensure we resolve the correct ctor
                obj.Blap = blap;
                return obj;
            }
        }
    }
}
