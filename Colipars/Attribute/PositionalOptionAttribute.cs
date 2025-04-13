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

        public string Description { get; set; } = string.Empty;

        string IOption.Name => $"[{Position}]";

        string IOption.Alias => string.Empty;

        bool IOption.Required => true;

        public PositionalOptionAttribute(int position)
        {
            this.Position = position;
        }
    }
}
