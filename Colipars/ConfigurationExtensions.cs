using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Reflection;
using Colipars.Attribute;

namespace Colipars
{
    public static class ConfigurationExtensions
    {
        public static void UseAsDefault<T>(this AttributeConfiguration configuration)
        {
            configuration.DefaultVerb = AttributeSettingsProvider.GetVerbFromType(typeof(T));
        }
    }
}
