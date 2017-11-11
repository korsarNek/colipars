using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Reflection;

namespace Colipars.Attribute
{
    /// <summary>
    /// Converts values using TypeConverters.
    /// </summary>
    class ValueTypeConverter : IValueConverter
    {
        public CultureInfo CultureInfo { get; }

        public ValueTypeConverter(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
        }

        public object ConvertFromString(InstanceOption option, string text)
        {
            TypeConverter converter;

            var collectionConverter = option.PropertyInfo.GetCustomAttribute<CollectionTypeConverterAttribute>();
            if (collectionConverter != null)
                converter = (TypeConverter)Activator.CreateInstance(Type.GetType(collectionConverter.ConverterTypeName));
            else
                converter = TypeDescriptor.CreateProperty(option.PropertyInfo.DeclaringType, option.PropertyInfo.Name, option.GetValueType()).Converter;
            
            return converter.ConvertFromString(null, CultureInfo, text);
        }
    }
}
