using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    public interface IValueConverter
    {
        object ConvertFromString(InstanceOption option, string text);
    }
}
