﻿using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NamedCollectionOptionAttribute : System.Attribute, IOption
    {
        public string Name { get; }

        public string Alias { get; set; }

        public int MinimumCount { get; set; }

        public string Description { get; set; }

        bool IOption.Required => MinimumCount > 0;

        public NamedCollectionOptionAttribute(string name)
        {
            this.Name = name;
        }
    }
}