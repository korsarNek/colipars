using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Colipars.Internal;

namespace Colipars.Attribute.Method
{
    public class AttributeParser : IParser<AttributeParseResult>
    {
        private readonly IHelpPresenter _helpPresenter;
        private readonly AttributeHandler _attributeHandler;

        public AttributeParser(AttributeConfiguration configuration, IParameterFormatter parameterFormatter, IValueConverter valueConverter, IHelpPresenter helpPresenter)
        {
            Configuration = configuration;
            _attributeHandler = new AttributeHandler(parameterFormatter,
                (option, value) =>
                {
                    var parameterValueOption = configuration.GetParameterValueOption(option);
                    return valueConverter.ConvertFromString(option, parameterValueOption.ParameterInfo, AttributeHandler.GetValueType(parameterValueOption.Option, parameterValueOption.ParameterInfo.ParameterType), value);
                },
                (option) => configuration.GetParameterValueOption(option).ParameterInfo.ParameterType
            );
            _helpPresenter = helpPresenter;
        }

        public AttributeConfiguration Configuration { get; }

        #region ShowHelp

        public AttributeParseResult ShowHelp(IVerb verb)
        {
            _helpPresenter.Present(verb);

            return AttributeParseResult.CreateHelpRequest(verb, Configuration.Services.GetService<ErrorHandler>());
        }

        public AttributeParseResult ShowHelp()
        {
            _helpPresenter.Present();

            return AttributeParseResult.CreateHelpRequest(null, Configuration.Services.GetService<ErrorHandler>());
        }

        #endregion

        public AttributeParseResult Parse(IEnumerable<string> args)
        {
            //TODO: add test for this case with explanation why.
            args = args.Where((x) => !string.IsNullOrEmpty(x));

            var firstParam = args.FirstOrDefault();
            IVerb verb;
            if (firstParam == null)
            {
                if (Configuration._defaultMethod != null)
                {
                    verb = new VerbAttribute(Configuration._defaultMethod.Name);
                    Configuration.Process(verb, Configuration._defaultMethod, Configuration._defaultMethod.DeclaringType.GetTypeInfo());
                }
                else if (Configuration.ShowHelpOnMissingVerb)
                    return ShowHelp();
                else
                    return AttributeParseResult.CreateErrorResult(null, Configuration.Services.GetService<ErrorHandler>(), [new VerbIsMissingError()]);
            }
            else if (Configuration.HelpArguments.Contains(firstParam))
                return ShowHelp();
            else
            {
                verb = Configuration.Verbs.FirstOrDefault((x) => x.Name == firstParam);
                if (verb == null)
                    if (Configuration._defaultMethod != null)
                    {
                        verb = new VerbAttribute(Configuration._defaultMethod.Name);
                        Configuration.Process(verb, Configuration._defaultMethod, Configuration._defaultMethod.DeclaringType.GetTypeInfo());
                    }
                    else
                        return AttributeParseResult.CreateErrorResult(null, Configuration.Services.GetService<ErrorHandler>(), [new UnknownVerbError(firstParam)]);
                else
                    args = args.Skip(1);
            }

            if (args.Any((x) => Configuration.HelpArguments.Contains(x)))
                return ShowHelp(verb);

            if (!_attributeHandler.TryProcessArguments(verb, Configuration.GetOptions(verb), args, out var optionValues, out var errors))
            {
                return AttributeParseResult.CreateErrorResult(verb, Configuration.Services.GetService<ErrorHandler>(), errors);
            }

            var verbData = Configuration.GetVerbData(verb);
            foreach (var optionValue in optionValues)
            {
                Configuration.GetParameterValueOption(optionValue.Option).SetValue(optionValue.Value);
            }

            return AttributeParseResult.CreateSuccessResult(verb, Configuration.Services.GetService<ErrorHandler>(), verbData.Method, verbData.Instance, [..verbData.Parameters.Select((x) => x.Value)]);
        }

        IParseResult IParser.Parse(IEnumerable<string> args)
        {
            return Parse(args);
        }

        IParseResult IParser.ShowHelp(IVerb verb)
        {
            return ShowHelp(verb);
        }

        IParseResult IParser.ShowHelp()
        {
            return ShowHelp();
        }
    }
}
