using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public sealed class PositionalOptionAttribute : System.Attribute, IOption
    {
        public int Position { get; }

        public string Description { get; set; } = String.Empty;

        string IOption.Name => $"[{Position}]";

        string IOption.Alias => String.Empty;

        bool IOption.Required => true;

        public PositionalOptionAttribute(int position)
        {
            this.Position = position;
        }
    }
}
