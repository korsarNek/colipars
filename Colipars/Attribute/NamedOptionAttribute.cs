using Colipars.Internal;
using System;

namespace Colipars.Attribute
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class NamedOptionAttribute : System.Attribute, IOption
    {
        public string Name { get; }

        public string Alias { get; set; } = String.Empty;

        public bool Required { get; set; }

        public string Description { get; set; } = String.Empty;

        public NamedOptionAttribute(string name)
        {
            this.Name = name;
        }
    }
}
