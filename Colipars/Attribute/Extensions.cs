using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Attribute
{
    internal static class Extensions
    {
        public static T? GetCustomAttribute<T>(this ICustomAttributeProvider attributeProvider) where T : class
        {
            return GetCustomAttribute<T>(attributeProvider, inherit: false);
        }

        public static T? GetCustomAttribute<T>(this ICustomAttributeProvider attributeProvider, bool inherit) where T : class
        {
            return (T)attributeProvider.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : class
        {
#pragma warning disable CS8619 // Die NULL-Zulässigkeit von Verweistypen im Wert entspricht nicht dem Zieltyp.
            return enumerable.Where((x) => x != null);
#pragma warning restore CS8619 // Die NULL-Zulässigkeit von Verweistypen im Wert entspricht nicht dem Zieltyp.
        }
    }
}
