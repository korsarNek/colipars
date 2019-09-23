using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Attribute
{
    public static class Extensions
    {
        public static T GetCustomAttribute<T>(this ICustomAttributeProvider attributeProvider)
        {
            return GetCustomAttribute<T>(attributeProvider, inherit: false);
        }

        public static T GetCustomAttribute<T>(this ICustomAttributeProvider attributeProvider, bool inherit)
        {
            return (T)attributeProvider.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }
    }
}
