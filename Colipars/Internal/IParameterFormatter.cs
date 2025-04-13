using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Internal
{
    public struct ParameterAndValue(string? parameter, string? value)
    {
        public string? Parameter => parameter;
        public string? Value => value;
    }

    public interface IParameterFormatter
    {
        string Format(string parameter);

        ParameterAndValue Parse(string parameter);
    }
}
