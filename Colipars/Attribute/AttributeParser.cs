using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.ObjectModel;
using Colipars.Internal;
using System.Collections;

namespace Colipars.Attribute
{
    public class AttributeParser : Parser<AttributeParseResult>
    {
        private IValueConverter _valueConverter;
        private IParameterFormatter _parameterFormatter;
        private IHelpPresenter _helpPresenter;

        public AttributeParser(AttributeSettings settings, AttributeConfiguration configuration, IParameterFormatter parameterFormatter, IValueConverter valueFormatter, IHelpPresenter helpPresenter)
            : base(settings, configuration)
        {
            _valueConverter = valueFormatter ?? throw new ArgumentNullException(nameof(valueFormatter));
            _parameterFormatter = parameterFormatter ?? throw new ArgumentNullException(nameof(parameterFormatter));
            _helpPresenter = helpPresenter ?? throw new ArgumentNullException(nameof(helpPresenter));
        }

        public new AttributeSettings Settings => (AttributeSettings)base.Settings;

        public new AttributeConfiguration Configuration => (AttributeConfiguration)base.Configuration;

        public virtual AttributeParseResult ShowHelp<T>()
        {
            return ShowHelp(AttributeSettingsProvider.GetVerbFromType(typeof(T)));
        }

        public override AttributeParseResult ShowHelp(IVerb verb)
        {
            _helpPresenter.Present(verb);

            return new AttributeParseResult(verb);
        }

        public override AttributeParseResult ShowHelp()
        {
            _helpPresenter.Present();

            return new AttributeParseResult(null);
        }

        protected override AttributeParseResult CreateErrorResult(IVerb verb, IEnumerable<IError> errors)
        {
            return new AttributeParseResult(verb, Configuration.Services.GetService<IErrorHandler>(), errors);
        }

        protected override AttributeParseResult ProcessArguments(IVerb verb, IEnumerable<string> arguments)
        {
            var options = Settings.GetInstanceOptions(verb);
            if (options == null)
                throw new InvalidOperationException($"Options for the verb \"{verb}\" are null.");

            var positionalOptions = options.Where((x) => x.Option is PositionalOptionAttribute).ToArray();
            var flagOptions = options.Where((x) => x.Option is FlagOptionAttribute).ToArray();
            var namedCollectionOptions = options.Where((x) => x.Option is NamedCollectionOptionAttribute).ToArray();
            var namedOptions = options.Except(positionalOptions).Except(flagOptions).Except(namedCollectionOptions).ToArray();

            int positionalArgumentCount = 0;
            var argsArray = arguments.ToArray();
            List<OptionAndValue> providedOptions = new List<OptionAndValue>();
            for (int i = 0; i < argsArray.Length; i++)
            {
                var argument = argsArray[i];
                var parameterName = _parameterFormatter.Parse(argument);

                if (HandleNamedOption(argsArray, ref i, parameterName, providedOptions, namedOptions)) { continue; }
                else if (HandleFlagOption(argument, parameterName, providedOptions, flagOptions)) { continue; }
                else if (HandlePositionOption(argument, parameterName, providedOptions, positionalOptions, ref positionalArgumentCount)) { continue; }
                else if (HandleNamedCollectionOption(argsArray, ref i, parameterName, providedOptions, namedCollectionOptions, flagOptions)) { continue; }
                {
                    return CreateErrorResult(verb, new OptionForArgumentNotFoundError(verb, argument, positionalArgumentCount));
                }
            }

            //check required parameters.
            foreach (var requiredOption in options.Where((o) => o.Option.Required))
            {
                var providedOption = providedOptions.FirstOrDefault((o) => o.Option == requiredOption.Option);
                if (providedOption == null)
                {
                    return CreateErrorResult(verb, new RequiredParameterMissingError(verb, requiredOption.Option.Name));
                }
                else if (providedOption.Option is NamedCollectionOptionAttribute collectionOption && ((IList)providedOption.Value).Count < collectionOption.MinimumCount)
                {
                    return CreateErrorResult(verb, new NotEnoughElementsError(verb, collectionOption.Name, collectionOption.MinimumCount));
                }
            }

            return CreateParseResult(verb, providedOptions);
        }

        private bool HandleFlagOption(string argument, string parameterName, List<OptionAndValue> providedOptions, IEnumerable<InstanceOption> flagOptions)
        {
            var instanceOption = GetFlagOption(parameterName, flagOptions);
            if (instanceOption?.Option is FlagOptionAttribute flagOption)
            {
                if (instanceOption.PropertyInfo.PropertyType != typeof(bool))
                {
                    //Use parameterName not the unparsed argument.
                    providedOptions.Add(new OptionAndValue(flagOption, _valueConverter.ConvertFromString(instanceOption, parameterName)));
                }
                else
                {
                    providedOptions.Add(new OptionAndValue(flagOption, true));
                }

                return true;
            }

            return false;
        }

        private bool HandlePositionOption(string argument, string parameterName, List<OptionAndValue> providedOptions, IEnumerable<InstanceOption> positionalOptions, ref int positionalArgumentCounter)
        {
            var positionalOption = positionalOptions.ElementAtOrDefault(positionalArgumentCounter);
            if (positionalOption != null)
            {
                positionalArgumentCounter++;
                providedOptions.Add(new OptionAndValue(positionalOption.Option, _valueConverter.ConvertFromString(positionalOption, argument)));

                return true;
            }

            return false;
        }

        private bool HandleNamedOption(string[] arguments, ref int argumentCounter, string parameterName, List<OptionAndValue> providedOptions, IEnumerable<InstanceOption> namedOptions)
        {
            var instanceOption = GetNamedOption(parameterName, namedOptions);
            if (instanceOption?.Option is NamedOptionAttribute namedOption)
            {
                argumentCounter++;
                providedOptions.Add(new OptionAndValue(namedOption, _valueConverter.ConvertFromString(instanceOption, arguments[argumentCounter])));

                return true;
            }

            return false;
        }

        private bool HandleNamedCollectionOption(string[] arguments, ref int argumentCounter, string parameterName, List<OptionAndValue> providedOptions, IEnumerable<InstanceOption> namedCollectionOptions, IEnumerable<InstanceOption> flagOptions)
        {
            var instanceOption = GetNamedOption(parameterName, namedCollectionOptions);
            if (instanceOption?.Option is NamedCollectionOptionAttribute namedOption)
            {
                List<object> list = new List<object>();
                while (argumentCounter + 1 < arguments.Length)
                {
                    argumentCounter++;
                    parameterName = _parameterFormatter.Parse(arguments[argumentCounter]);

                    if (GetNamedOption(parameterName, namedCollectionOptions) != null || GetFlagOption(parameterName, flagOptions) != null)
                    {
                        argumentCounter--;
                        break;
                    }

                    list.Add(_valueConverter.ConvertFromString(instanceOption, arguments[argumentCounter]));
                }

                providedOptions.Add(new OptionAndValue(namedOption, list));
                return true;
            }

            return false;
        }

        private InstanceOption GetNamedOption(string parameterName, IEnumerable<InstanceOption> namedOptions)
        {
            return namedOptions.FirstOrDefault((o) => o.Option.Name == parameterName || o.Option.Alias == parameterName);
        }

        private InstanceOption GetFlagOption(string parameterName, IEnumerable<InstanceOption> flagOptions)
        {
            return flagOptions.FirstOrDefault((o) => o.Option.Name == parameterName || parameterName.StartsWith(o.Option.Alias));
        }

        protected virtual AttributeParseResult CreateParseResult(IVerb verb, IEnumerable<OptionAndValue> providedOptions)
        {
            var instance = Settings.GetConstructor(verb).Invoke(new object[0]);
            var instanceOptions = Settings.GetInstanceOptions(verb);

            foreach (var instanceOption in instanceOptions)
            {
                var providedOption = providedOptions.FirstOrDefault((o) => o.Option == instanceOption.Option);
                if (providedOption == null)
                    continue;

                instanceOption.SetValue(instance, providedOption.Value);
            }

            return new AttributeParseResult(verb, instance);
        }
    }
}
