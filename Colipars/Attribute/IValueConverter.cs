using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Attribute
{
    public interface IValueConverter
    {
        object ConvertFromString(IOption option, ICustomAttributeProvider attributes, Type targetType, string text);
    }
}
