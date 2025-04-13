using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Colipars.Console
{
    class ParameterFormatter : IParameterFormatter
    {
        public string Format(string parameter)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "/" + parameter;

            if (parameter.Length == 1)
                return "-" + parameter;

            return "--" + parameter;
        }

        public ParameterAndValue Parse(string parameter)
        {
            int equalIndex = parameter.IndexOf('=');
            if (equalIndex != -1)
            {
                return new ParameterAndValue(parameter.Substring(0, equalIndex).TrimStart('-', '/'), parameter.Substring(equalIndex + 1));
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (parameter.StartsWith("/") || parameter.StartsWith("-"))
                {
                    return new ParameterAndValue(parameter.TrimStart('-', '/'), null);
                }
            }
            else
            {
                return new ParameterAndValue(parameter.TrimStart('-', '/'), null);
            }
            return new ParameterAndValue(null, parameter);
        }
    }
}
