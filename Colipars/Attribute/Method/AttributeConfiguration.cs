using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Colipars.Internal;

namespace Colipars.Attribute.Method
{
    public class AttributeConfiguration : Configuration
    {
        private Dictionary<IVerb, VerbData> _verbData;

        internal AttributeConfiguration(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public override IEnumerable<IVerb> Verbs => _verbData.Keys;

        public override IEnumerable<IOption> GetOptions(IVerb verb)
        {
            return _verbData[verb].ParameterOptions.Select((x) => x.Option).Where((x) => x != null);
        }

        public void UseAsDefault<T>(string methodName)
        {
            //TODO: throw exception if method doesn't exist or doesn't provide the attribute.
            DefaultVerb = GetVerbFromMethod(typeof(T).GetMethod(methodName));
        }

        internal void Initialize(IEnumerable<Type> optionTypes)
        {
            _verbData = new Dictionary<IVerb, VerbData>();

            foreach (var type in optionTypes)
            {
                var typeInfo = type.GetTypeInfo();

                foreach (var methodVerbPair in typeInfo.GetMethods(BindingFlags.Public | BindingFlags.Instance).Select((x) => new KeyValuePair<MethodInfo, IVerb>(x, GetVerbFromMethod(x))).Where((x) => x.Value != null))
                {
                    var parameterOptions = new List<ParameterValueOption>();
                    foreach (var parameter in methodVerbPair.Key.GetParameters())
                    {
                        var option = AttributeHandler.GetOption(this, methodVerbPair.Key.ToString(), parameter);
                        if (option == null && !parameter.HasDefaultValue)
                            throw new InvalidOperationException($"The parameter \"{parameter.Name}\" on \"{methodVerbPair.Key.Name}\" has no option attribute and no default value.");

                        parameterOptions.Add(new ParameterValueOption(parameter, option));
                    }

                    _verbData.Add(methodVerbPair.Value, new VerbData(methodVerbPair.Value, methodVerbPair.Key, () => AttributeHandler.GetConstructor(typeInfo).Invoke(new object[0]), parameterOptions));
                }
            }
        }

        public static IVerb GetVerbFromMethod(MethodInfo method) => method.GetCustomAttribute<VerbAttribute>(inherit: true);

        internal VerbData GetVerbData(IVerb verb)
        {
            return _verbData[verb];
        }

        internal ParameterValueOption GetParameterValueOption(IOption option)
        {
            foreach (var data in _verbData.Values)
            {
                var parameterValueOption = data.ParameterOptions.FirstOrDefault((x) => x.Option == option);
                if (parameterValueOption != null)
                    return parameterValueOption;
            }

            throw new KeyNotFoundException("No Parameter found for the option " + option.Name);
        }

        internal class VerbData
        {
            private Func<object> _instanceFactory;
            private object _instance = null;

            public IVerb Verb { get; set; }
            public MethodInfo Method { get; set; }
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
            public IEnumerable<ParameterValueOption> ParameterOptions { get; }

            public VerbData(IVerb verb, MethodInfo method, Func<object> instanceFactory, IEnumerable<ParameterValueOption> parameterOptions)
            {
                Verb = verb;
                Method = method;
                _instanceFactory = instanceFactory;
                ParameterOptions = parameterOptions;
            }

            public ParameterValueOption GetParameterValueOption(IOption option)
            {
                return ParameterOptions.First((x) => x.Option == option);
            }
        }

        internal class ParameterValueOption
        {
            private object _value = null;

            public ParameterInfo ParameterInfo { get; }
            public IOption Option { get; }

            public ParameterValueOption(ParameterInfo parameterInfo, IOption option)
            {
                ParameterInfo = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
                Option = option;

                if (ParameterInfo.HasDefaultValue)
                    _value = ParameterInfo.DefaultValue;
            }

            public object Value => _value;

            public void SetValue(object value)
            {
                if (AttributeHandler.IsCollectionAttribute(Option))
                {
                    //TODO: Create element of the correct type.
                    if (_value == null)
                    {
                        var constructor = ParameterInfo.ParameterType.GetConstructors().FirstOrDefault((x) => x.IsPublic && x.GetParameters().Length == 0);
                        if (constructor != null)
                            _value = constructor.Invoke(new object[0]);
                        else
                            _value = Activator.CreateInstance(typeof(List<>).MakeGenericType(AttributeHandler.GetValueType(Option, ParameterInfo.ParameterType)));
                    }

                    var list = (IList)_value;
                    foreach (var element in (IList)value)
                        list.Add(element);
                }
                else
                {
                    _value = value;
                }
            }
        }
    }
}
