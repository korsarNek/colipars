using Colipars.Internal;
using System;

namespace Colipars.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NamedOptionAttribute : System.Attribute, IOption
    {
        public string Name { get; }

        public string Alias { get; set; }

        public bool Required { get; set; }

        public string Description { get; set; }

        public NamedOptionAttribute(string name)
        {
            this.Name = name;
        }
    }
}
