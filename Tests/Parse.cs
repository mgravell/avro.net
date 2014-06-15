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
            foreach(string file in Directory.EnumerateFiles("Schemas"))
            {
                Console.WriteLine("Parsing: " + file);
                var text = File.ReadAllText(file);
                var model = Avro.AvroSchema.Parse(text);
            }
            
        }
    }
}
