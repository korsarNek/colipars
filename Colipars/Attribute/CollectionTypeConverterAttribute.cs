using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class CollectionTypeConverterAttribute : System.Attribute
    {
        public CollectionTypeConverterAttribute(Type type)
        {
            ConverterTypeName = type.AssemblyQualifiedName ?? throw new ArgumentNullException(nameof(type));
        }

        public CollectionTypeConverterAttribute(string typeName)
        {
            ConverterTypeName = typeName;
        }

        public string ConverterTypeName { get; }
    }
}
