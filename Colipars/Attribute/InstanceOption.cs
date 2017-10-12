using Colipars.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Colipars.Attribute
{
    public class InstanceOption
    {
        public PropertyInfo PropertyInfo { get; }

        public IOption Option { get; }

        public InstanceOption(PropertyInfo propertyInfo, IOption option)
        {
            this.PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
            this.Option = option ?? throw new ArgumentNullException(nameof(option));
        }

        public void SetValue(object instance, object value)
        {
            if (Option is NamedCollectionOptionAttribute)
            {
                //TODO: use ICollection for this case like the ValueTypeConverter, instead of IList.
                //that way we don't demand both IList and ICollection<>
                var collection = (IList)PropertyInfo.GetValue(instance);

                foreach (var element in (IList)value)
                    collection.Add(element);
            }
            else
            {
                PropertyInfo.SetValue(instance, value);
            }
        }

        public Type GetValueType()
        {
            if (Option is NamedCollectionOptionAttribute namedCollection)
            {
                Type interfaceType = new[] { PropertyInfo.PropertyType }.Concat(PropertyInfo.PropertyType.GetInterfaces()).FirstOrDefault((x) => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICollection<>));
                if (interfaceType == null)
                    throw new InvalidOperationException($"The property \"{PropertyInfo.Name}\" on \"{PropertyInfo.DeclaringType}\" is marked as NamedCollectionOption but doesn't use a type that implements {typeof(ICollection<>)}.");

                return interfaceType.GetGenericArguments()[0];
            }

            return PropertyInfo.PropertyType;
        }
    }
}
