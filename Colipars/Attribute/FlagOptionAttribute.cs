using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class FlagOptionAttribute : System.Attribute, IOption
    {
        public string Name { get; }

        public string ShortHand { get; set; }

        public bool Required { get; set; }

        public string Description { get; set; }

        string IOption.Alias => ShortHand;

        public FlagOptionAttribute(string name)
        {
            Name = name;
        }
    }
}
