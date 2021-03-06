using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class VerbAttribute : System.Attribute, IVerb, IEquatable<VerbAttribute>
    {
        public string Name { get; }

        public string Description { get; set; } = String.Empty;

        public VerbAttribute(string name)
        {
            this.Name = name;
        }

        public bool Equals(VerbAttribute other)
        {
            if (other == null)
                return false;

            return other.Name == this.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is VerbAttribute verb)
                return Equals(verb);

            return base.Equals(obj);
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
    }
}
