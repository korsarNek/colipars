using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Attribute
{
    public class ValueTypeConverter : IValueConverter
    {
        private CultureInfo _cultureInfo;

        public ValueTypeConverter(CultureInfo cultureInfo)
        {
            _cultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
        }

        public object ConvertFromString(IOption option, ICustomAttributeProvider attributes, Type targetType, string text)
        {
            TypeConverter converter;
            var collectionConverter = attributes.GetCustomAttribute<CollectionTypeConverterAttribute>();
            if (collectionConverter != null)
                converter = (TypeConverter)Activator.CreateInstance(Type.GetType(collectionConverter.ConverterTypeName));
            else
            {
                var typeConverterAttribute = attributes.GetCustomAttribute<TypeConverterAttribute>();
                if (typeConverterAttribute != null)
                    converter = (TypeConverter)Activator.CreateInstance(Type.GetType(typeConverterAttribute.ConverterTypeName));
                else
                    converter = TypeDescriptor.GetConverter(targetType);
            }

            if (converter == null)
                throw new InvalidOperationException($"Can not get a TypeConverter for \"{targetType}\".");

            return converter.ConvertFromString(null, _cultureInfo, text);
        }
    }
}
