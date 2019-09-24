using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class NamedCollectionOptionAttribute : System.Attribute, IOption
    {
        public string Name { get; }

        public string Alias { get; set; } = String.Empty;

        public int MinimumCount { get; set; }

        public string Description { get; set; } = String.Empty;

        bool IOption.Required => MinimumCount > 0;

        public NamedCollectionOptionAttribute(string name)
        {
            this.Name = name;
        }
    }
}
