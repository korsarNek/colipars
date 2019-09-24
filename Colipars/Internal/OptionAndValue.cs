using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Internal
{
    public class OptionAndValue
    {
        public IOption Option { get; }

        public object? Value { get; }

        public OptionAndValue(IOption option, object? value)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
            Value = value;
        }

        public IList ValueAsList()
        {
            if (Value == null)
                throw new InvalidCastException("Trying to cast null to IList.");

            return (IList)Value;
        }
    }
}
