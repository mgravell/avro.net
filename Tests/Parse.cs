using Avro;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class Parse
    {
        [Test]
        public void DoParse()
        {
            foreach(string file in Directory.EnumerateFiles("Schemas", "*.json"))
            {
                Console.WriteLine("Parsing: " + file);
                var text = File.ReadAllText(file);
                var model = AvroType.Parse(text);

                string schema = model.GetSchema();
                File.WriteAllText(Path.ChangeExtension(file, "export"), schema);
            }
            
        }
    }
}
