using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Internal
{
    public class OptionAndValue
    {
        public IOption Option { get; }

        public object Value { get; }

        public OptionAndValue(IOption option, object value)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
            Value = value;
        }
    }
}
