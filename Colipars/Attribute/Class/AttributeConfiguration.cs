using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Colipars.Internal;
using System.Linq;

namespace Colipars.Attribute.Class
{
    public class AttributeConfiguration : Configuration
    {
        readonly Dictionary<IVerb, VerbData>  _verbsData = new Dictionary<IVerb, VerbData>();

        internal AttributeConfiguration(IServiceProvider serviceProvider, IEnumerable<Type> optionTypes)
            : base(serviceProvider)
        {
            foreach (var type in optionTypes)
            {
                var typeInfo = type.GetTypeInfo();
                var verb = GetVerbFromType(type);

                //get options
                var options = new List<OptionProperty>();
                foreach (var property in typeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var option = AttributeHandler.GetOption(this, type.Name, property);
                    if (option != null)
                    {
                        options.Add(new OptionProperty(option, property));
                    }
                }

                _verbsData.Add(verb, new VerbData(verb, () => AttributeHandler.GetConstructor(typeInfo).Invoke(new object[0]), options));
            }
        }

        public override IEnumerable<IVerb> Verbs => _verbsData.Keys;

        public void UseAsDefault<T>()
        {
            DefaultVerb = GetVerbFromType(typeof(T));
        }

        public override IEnumerable<IOption> GetOptions(IVerb verb) => _verbsData[verb].OptionProperties.Select((x) => x.Option);

        internal VerbData GetVerbData(IVerb verb) => _verbsData[verb];

        internal OptionProperty GetOptionProperty(IOption option)
        {
            foreach (var verbData in _verbsData.Values)
            {
                var optionProperty = verbData.OptionProperties.FirstOrDefault((x) => x.Option == option);
                if (optionProperty != null)
                    return optionProperty;
            }

            throw new KeyNotFoundException("No Property found for the option " + option.Name);
        }

        public static IVerb GetVerbFromType(Type type) => type.GetTypeInfo().GetCustomAttribute<VerbAttribute>() ?? new VerbAttribute(type.Name);

        internal class VerbData
        {
            readonly Func<object> _instanceFactory;
            private object? _instance = null;

            public IVerb Verb { get; set; }

            /// <summary>
            /// The instance of the class on which the verb is defined.
            /// </summary>
            public object Instance
            {
                get
                {
                    if (_instance != null)
                        return _instance;

                    _instance = _instanceFactory();
                    return _instance;
                }
            }
            public IEnumerable<OptionProperty> OptionProperties { get; }

            public VerbData(IVerb verb, Func<object> instanceFactory, IEnumerable<OptionProperty> optionProperties)
            {
                Verb = verb;
                _instanceFactory = instanceFactory;
                OptionProperties = optionProperties;
            }
        }

        internal class OptionProperty
        {
            public IOption Option { get; private set; }
            public PropertyInfo PropertyInfo { get; private set; }

            public OptionProperty(IOption option, PropertyInfo propertyInfo)
            {
                Option = option;
                PropertyInfo = propertyInfo;
            }

            /// <summary>
            /// Sets the value of the property, or if it has a NamedCollectionOption, adds it to the collection of the property value.
            /// </summary>
            /// <param name="instance"></param>
            /// <param name="value"></param>
            public void SetValue(object instance, object? value)
            {
                if (AttributeHandler.IsCollectionAttribute(Option))
                {
                    if (value == null) throw new InvalidOperationException($"The target value for the property \"{PropertyInfo.Name}\" on \"{PropertyInfo.DeclaringType}\" is null, but it is marked as a collection.");
                    var propertyValue = PropertyInfo.GetValue(instance) ?? throw new InvalidOperationException($"The property \"{PropertyInfo.Name}\" on \"{PropertyInfo.DeclaringType}\" must return an instance.");

                    //TODO: use ICollection for this case, instead of IList.
                    //that way we don't demand both IList and ICollection<>
                    var list = (IList)propertyValue;

                    list.Clear();
                    foreach (var element in (IList)value)
                        list.Add(element);
                }
                else
                {
                    PropertyInfo.SetValue(instance, value);
                }
            }
        }
    }
}
