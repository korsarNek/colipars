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
#if NETFRAMEWORK4_5
            return "/" + parameter;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "/" + parameter;

            if (parameter.Length == 1)
                return "-" + parameter;

            return "--" + parameter;
#endif
        }

        public string Parse(string parameter)
        {
            return parameter.TrimStart('-', '/');
        }
    }
}
