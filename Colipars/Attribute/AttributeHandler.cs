using Colipars.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Attribute
{
    //TODO: use this as base for the attributeParsers
    class AttributeHandler
    {
        private readonly Func<IOption, string, object> _valueConverter;
        private readonly Func<IOption, Type> _memberTypeFromOption;
        private readonly IParameterFormatter _parameterFormatter;

        public AttributeHandler(IParameterFormatter parameterFormatter, Func<IOption, string, object> valueConverter, Func<IOption, Type> memberTypeFromOption)
        {
            //TODO: instead of func, provide an interface for the conversion.
            _valueConverter = valueConverter;
            _memberTypeFromOption = memberTypeFromOption;
            _parameterFormatter = parameterFormatter;
        }

        public bool TryProcessArguments(IVerb verb, IEnumerable<IOption> options, IEnumerable<string> arguments, out IEnumerable<OptionAndValue> optionValues, out IEnumerable<IError> errors)
        {
            var positionalOptions = options.OfType<PositionalOptionAttribute>().ToArray();
            var flagOptions = options.OfType<FlagOptionAttribute>().ToArray();
            var namedCollectionOptions = options.OfType<NamedCollectionOptionAttribute>().ToArray();
            var namedOptions = options.OfType<NamedOptionAttribute>().ToArray();

            int positionalArgumentCount = 0;
            var argsArray = arguments.ToArray();
            List<OptionAndValue> providedOptions = [];
            for (int i = 0; i < argsArray.Length; i++)
            {
                var argument = argsArray[i];
                var parameterAndValue = _parameterFormatter.Parse(argument);

                if (HandleNamedOption(argsArray, ref i, parameterAndValue, providedOptions, namedOptions)) { continue; }
                else if (HandleFlagOption(parameterAndValue, providedOptions, flagOptions)) { continue; }
                else if (HandlePositionOption(argument, providedOptions, positionalOptions, ref positionalArgumentCount)) { continue; }
                else if (HandleNamedCollectionOption(argsArray, ref i, parameterAndValue, providedOptions, namedCollectionOptions, flagOptions, namedOptions)) { continue; }
                {
                    optionValues = [];
                    errors = [new OptionForArgumentNotFoundError(verb, argument, positionalArgumentCount)];
                    return false;
                }
            }

            //check required parameters.
            foreach (var requiredOption in options.Where((o) => o.Required))
            {
                var providedOption = providedOptions.FirstOrDefault((o) => o.Option == requiredOption);
                if (providedOption == null)
                {
                    optionValues = [];
                    errors = [new RequiredParameterMissingError(verb, requiredOption.Name)];
                    return false;
                }
                else if (providedOption.Option is NamedCollectionOptionAttribute collectionOption && providedOption.ValueAsList().Count < collectionOption.MinimumCount)
                {
                    optionValues = [];
                    errors = [new NotEnoughElementsError(verb, collectionOption.Name, collectionOption.MinimumCount)];
                    return false;
                }
            }

            optionValues = providedOptions;
            errors = [];
            return true;
        }

        private bool HandleFlagOption(ParameterAndValue parameterAndValue, List<OptionAndValue> providedOptions, IEnumerable<FlagOptionAttribute> flagOptions)
        {
            if (parameterAndValue.Value != null || parameterAndValue.Parameter == null)
                return false;

            var flagOption = GetFlagOption(parameterAndValue.Parameter, flagOptions);
            if (flagOption != null)
            {
                //GetValueType handelt nur NamedCollections, also wofür brauche ich das hier?
                if (_memberTypeFromOption(flagOption) != typeof(bool))
                {
                    //Use parameterName not the unparsed argument.
                    providedOptions.Add(new OptionAndValue(flagOption, _valueConverter(flagOption, parameterAndValue.Parameter)));
                }
                else
                {
                    providedOptions.Add(new OptionAndValue(flagOption, true));
                }

                return true;
            }

            return false;
        }

        private bool HandlePositionOption(string argument, List<OptionAndValue> providedOptions, IEnumerable<PositionalOptionAttribute> positionalOptions, ref int positionalArgumentCounter)
        {
            var positionalOption = positionalOptions.ElementAtOrDefault(positionalArgumentCounter);
            if (positionalOption != null)
            {
                positionalArgumentCounter++;
                providedOptions.Add(new OptionAndValue(positionalOption, _valueConverter(positionalOption, argument)));

                return true;
            }

            return false;
        }

        private bool HandleNamedOption(string[] arguments, ref int argumentCounter, ParameterAndValue parameterAndValue, List<OptionAndValue> providedOptions, IEnumerable<NamedOptionAttribute> namedOptions)
        {
            if (parameterAndValue.Parameter != null)
            {
                var namedOption = GetOption(parameterAndValue.Parameter, namedOptions);
                if (namedOption != null)
                {
                    if (parameterAndValue.Value != null)
                    {
                        providedOptions.Add(new OptionAndValue(namedOption, _valueConverter(namedOption, parameterAndValue.Value)));

                        return true;
                    }
                    else if (arguments.Length > argumentCounter + 1)
                    {
                        argumentCounter++;
                        providedOptions.Add(new OptionAndValue(namedOption, _valueConverter(namedOption, arguments[argumentCounter])));

                        return true;
                    }
                }
            }

            return false;
        }

        private bool HandleNamedCollectionOption(string[] arguments, ref int argumentCounter, ParameterAndValue parameterAndValue, List<OptionAndValue> providedOptions, IEnumerable<NamedCollectionOptionAttribute> namedCollectionOptions, IEnumerable<FlagOptionAttribute> flagOptions, IEnumerable<NamedOptionAttribute> namedOptions)
        {
            if (parameterAndValue.Parameter == null)
                return false;

            var namedCollectionOption = GetOption(parameterAndValue.Parameter, namedCollectionOptions);
            if (namedCollectionOption != null)
            {
                // option has already been added before, then we extend it.
                IList list;
                var existingOption = providedOptions.Find(o => o.Option == namedCollectionOption);
                if (existingOption != null)
                {
                    list = existingOption.ValueAsList();
                }
                else
                {
                    list = new List<object>();
                }
                
                if (parameterAndValue.Value != null)
                {
                    foreach (var element in parameterAndValue.Value.Split(','))
                    {
                        list.Add(_valueConverter(namedCollectionOption, element));
                    }
                }
                else
                {
                    while (argumentCounter + 1 < arguments.Length)
                    {
                        argumentCounter++;
                        var subParameterAndValue = _parameterFormatter.Parse(arguments[argumentCounter]);

                        if (subParameterAndValue.Parameter != null && (GetOption(subParameterAndValue.Parameter, namedCollectionOptions) != null || GetOption(subParameterAndValue.Parameter, namedOptions) != null || GetFlagOption(subParameterAndValue.Parameter, flagOptions) != null))
                        {
                            argumentCounter--;
                            break;
                        }

                        list.Add(_valueConverter(namedCollectionOption, arguments[argumentCounter]));
                    }
                }

                if (existingOption == null)
                    providedOptions.Add(new OptionAndValue(namedCollectionOption, list));
                
                return true;
            }

            return false;
        }

        private TOption GetOption<TOption>(string parameterName, IEnumerable<TOption> options) where TOption : IOption
        {
            return options.FirstOrDefault((o) => o.Name == parameterName || o.Alias == parameterName);
        }

        private FlagOptionAttribute GetFlagOption(string parameterName, IEnumerable<FlagOptionAttribute> flagOptions)
        {
            return flagOptions.FirstOrDefault((o) => o.Name == parameterName || parameterName.StartsWith(o.ShortHand));
        }

        public static bool IsCollectionAttribute(IOption option)
        {
            return option is NamedCollectionOptionAttribute;
        }

        public static ConstructorInfo GetConstructor(TypeInfo typeInfo)
        {
            //check if type is valid
            if (typeInfo.IsGenericTypeDefinition)
                throw new InvalidOperationException($"\"{typeInfo}\" is a generic type definition.");

            var constructor = typeInfo.GetConstructors().FirstOrDefault((x) => x.IsPublic && x.GetParameters().Length == 0);
            if (constructor == null)
                throw new InvalidOperationException($"\"{typeInfo}\" doesn't have a public parameterless constructor.");

            return constructor;
        }

        /// <summary>
        /// Returns a supported <see cref="IOption"/> Attribute from the given AttributeProvider, or null if no valid supported option has been found.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="parentIdentifier"></param>
        /// <param name="attributeProvider"></param>
        /// <exception cref="AmbiguousMatchException">Gets thrown if an option name conflicts with help argument name.</exception>
        /// <returns></returns>
        public static IOption? GetOption(Configuration configuration, string parentIdentifier, ICustomAttributeProvider attributeProvider)
        {
            return Validate(configuration, parentIdentifier,
                attributeProvider.GetCustomAttribute<NamedOptionAttribute>() ??
                attributeProvider.GetCustomAttribute<PositionalOptionAttribute>() ??
                attributeProvider.GetCustomAttribute<NamedCollectionOptionAttribute>() ??
                (IOption?)attributeProvider.GetCustomAttribute<FlagOptionAttribute>()
            );
        }

        /// <summary>
        /// Gets the value type of the given type.
        /// That is either the given type, or in case that the option is for a collection, the type of the collection elements.
        /// </summary>
        /// <param name="option"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetValueType(IOption option, Type type)
        {
            if (IsCollectionAttribute(option))
            {
                var collectionType = type.GetInterfaces().Concat(new[] { type }).FirstOrDefault((x) => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (collectionType != null)
                    return collectionType.GetGenericArguments()[0];
            }

            return type;
        }

        private static IOption? Validate(Configuration configuration, string parentIdentifier, IOption? option)
        {
            if (option == null)
                return null;

            if (configuration.HelpArguments.Contains(option.Name))
                throw new AmbiguousMatchException($"The help argument \"{option.Name}\" is ambigous with option of the same name in \"{parentIdentifier}\"");

            return option;
        }
    }
}
