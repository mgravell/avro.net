using System;
using System.ComponentModel;

namespace Avro
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    [ImmutableObject(true)]
    public sealed class AvroContractAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    [ImmutableObject(true)]
    public sealed class AvroFieldAttribute : Attribute
    {
        private readonly int fieldNumber;

        public int FieldNumber { get { return fieldNumber; } }
        public AvroFieldAttribute(int fieldNumber)
        {
            this.fieldNumber = fieldNumber;
        }
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    [ImmutableObject(true)]
    public sealed class AvroConstructor : Attribute
    {
    }
}
