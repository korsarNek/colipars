using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class VerbAttribute : System.Attribute
    {
        public string Name { get; }

        public VerbAttribute()
        {

        }

        public VerbAttribute(string name)
        {
            this.Name = name;
        }
    }
}
