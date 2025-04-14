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
    public partial class AttributeConfiguration : Configuration
    {
        private readonly Dictionary<IVerb, VerbData> _verbData = [];
        private readonly IServiceProvider _serviceProvider;
        internal MethodInfo? _defaultMethod;
        private object? _instance;

        internal AttributeConfiguration(IServiceProvider serviceProvider, IEnumerable<Type> optionTypes, object? instance = null)
        {
            _serviceProvider = serviceProvider;
            _instance = instance;
            foreach (var type in optionTypes)
            {
                var typeInfo = type.GetTypeInfo();

                foreach (var methodVerbPair in typeInfo.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Select((x) => new KeyValuePair<MethodInfo, IVerb?>(x, GetVerbFromMethod(x))).Where((x) => x.Value != null))
                {
                    Process(methodVerbPair.Value!, methodVerbPair.Key, typeInfo);
                }
            }
        }

        public override IServiceProvider Services => _serviceProvider;

        public override IEnumerable<IVerb> Verbs => _verbData.Keys;

        public override IEnumerable<IOption> GetOptions(IVerb verb)
        {
            return _verbData[verb].ParameterOptions.Select((x) => x.Option).WhereNotNull();
        }

        internal void Process(IVerb verb, MethodInfo method, TypeInfo typeInfo)
        {
            var parameterOptions = new List<IParameterValue>();
            foreach (var parameter in method.GetParameters())
            {
                var option = AttributeHandler.GetOption(this, method.ToString(), parameter);
                if (option == null)
                    if (parameter.HasDefaultValue)
                        parameterOptions.Add(new DefaultValueParameter(parameter));
                    else
                        throw new InvalidOperationException($"The parameter \"{parameter.Name}\" on \"{method.DeclaringType.FullName + ":" + method.Name}\" for the verb \"{verb.Name}\" has neither an option, nor a default value.");
                else
                    parameterOptions.Add(new ParameterValueOption(parameter, option, method));
            }

            _verbData.Add(verb, new VerbData(verb, method, () =>
            {
                if (_instance != null)
                    return _instance;

                return AttributeHandler.GetConstructor(typeInfo).Invoke([]);
            }, parameterOptions));
        }

        /// <summary>
        /// Sets the default method to use if none was provided through arguments.
        /// 
        /// The referenced method must be public, otherwise it might have been removed from the compiled code,
        /// or use one of the other UseAsDefault overloads.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodName"></param>
        /// <exception cref="System.MissingMethodException"></exception>
        public void UseAsDefault<T>(string methodName)
        {
            var method = typeof(T).GetMethod(methodName);
            if (method == null)
                throw new MissingMethodException(typeof(T).FullName, methodName);

            _defaultMethod = method;
        }

        // Other UseAsDefault come from CodeGenerator

        public static IVerb? GetVerbFromMethod(MethodInfo method) => method.GetCustomAttribute<VerbAttribute>(inherit: true);

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
            private readonly Func<object> _instanceFactory;
            private object? _instance = null;

            public IVerb Verb { get; set; }
            public MethodInfo Method { get; set; }
            public object? Instance
            {
                get
                {
                    if (_instance != null)
                        return _instance;

                    if (Method.IsStatic)
                        return null;
                    
                    _instance = _instanceFactory();
                    return _instance;
                }
            }

            public IEnumerable<IParameterValue> Parameters { get; }

            public IEnumerable<ParameterValueOption> ParameterOptions => Parameters.OfType<ParameterValueOption>();

            public VerbData(IVerb verb, MethodInfo method, Func<object> instanceFactory, IEnumerable<IParameterValue> parameterOptions)
            {
                Verb = verb;
                Method = method;
                _instanceFactory = instanceFactory;
                Parameters = parameterOptions;
            }
        }

        internal interface IParameterValue
        {
            public ParameterInfo ParameterInfo { get; }

            public object? Value { get; }
        }

        internal class ParameterValueOption : IParameterValue
        {
            public ParameterInfo ParameterInfo { get; }
            public IOption Option { get; }

            public object? Value { get; private set; } = null;

            private readonly MethodInfo _methodInfo;

            public ParameterValueOption(ParameterInfo parameterInfo, IOption option, MethodInfo methodInfo)
            {
                ParameterInfo = parameterInfo;
                Option = option;
                _methodInfo = methodInfo;
            }

            private string MethodName => _methodInfo.DeclaringType.FullName + ":" + _methodInfo.Name;

            public void SetValue(object? value)
            {
                if (AttributeHandler.IsCollectionAttribute(Option))
                {
                    if (value == null) throw new InvalidOperationException($"The target value for the property \"{ParameterInfo.Name}\" on \"{MethodName}\" is null, but it is marked as a collection.");

                    //TODO: Create element of the correct type.
                    if (Value == null)
                    {
                        var constructor = ParameterInfo.ParameterType.GetConstructors().FirstOrDefault((x) => x.IsPublic && x.GetParameters().Length == 0);
                        if (constructor != null)
                            Value = constructor.Invoke(new object[0]);
                        else
                            Value = Activator.CreateInstance(typeof(List<>).MakeGenericType(AttributeHandler.GetValueType(Option, ParameterInfo.ParameterType)));
                    }

                    var list = (IList)Value;

                    list.Clear();
                    foreach (var element in (IList)value)
                        list.Add(element);
                }
                else
                {
                    Value = value;
                }
            }
        }

        internal class DefaultValueParameter : IParameterValue
        {
            public ParameterInfo ParameterInfo { get; }

            public object? Value { get; private set; }

            public DefaultValueParameter(ParameterInfo parameterInfo)
            {
                ParameterInfo = parameterInfo;
                Value = parameterInfo.DefaultValue;
            }
        }
    }
}
