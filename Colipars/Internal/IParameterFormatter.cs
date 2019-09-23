using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Internal
{
    public interface IParameterFormatter
    {
        string Format(string parameter);

        string Parse(string parameter);
    }
}
