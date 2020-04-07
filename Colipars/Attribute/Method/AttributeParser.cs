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
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _attributeHandler = new AttributeHandler(configuration, parameterFormatter,
                (option, value) =>
                {
                    var parameterValueOption = configuration.GetParameterValueOption(option);
                    return valueConverter.ConvertFromString(option, parameterValueOption.ParameterInfo, AttributeHandler.GetValueType(parameterValueOption.Option, parameterValueOption.ParameterInfo.ParameterType), value);
                },
                (option) => configuration.GetParameterValueOption(option).ParameterInfo.ParameterType
            );
            _helpPresenter = helpPresenter ?? throw new ArgumentNullException(nameof(helpPresenter));
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
            if (!_attributeHandler.TryParseSelectedVerb(Configuration.Verbs, ref args, out var error, out var verb, out var requestedHelp))
            {
                if (requestedHelp)
                    if (verb == null)
                        return ShowHelp();
                    else
                        return ShowHelp(verb);
                else if(error == null)
                    throw new LogicException("The attribute handled didn't parse the result and no help was requested, but no error was generated.");
                else
                    return AttributeParseResult.CreateErrorResult(verb, Configuration.Services.GetService<ErrorHandler>(), new[] { error });
            }

            IVerb safeVerb = verb ?? throw new InvalidOperationException("The attribute handler parsed the verb, but didn't return it.");

            if (!_attributeHandler.TryProcessArguments(safeVerb, Configuration.GetOptions(safeVerb), args, out var optionValues, out var errors))
            {
                return AttributeParseResult.CreateErrorResult(verb, Configuration.Services.GetService<ErrorHandler>(), errors);
            }

            var verbData = Configuration.GetVerbData(safeVerb);
            foreach (var optionValue in optionValues)
            {
                Configuration.GetParameterValueOption(optionValue.Option).SetValue(optionValue.Value);
            }

            return AttributeParseResult.CreateSuccessResult(safeVerb, Configuration.Services.GetService<ErrorHandler>(), verbData.Method, verbData.Instance, verbData.Parameters.Select((x) => x.Value).ToArray());
        }

        IParseResult IParser.Parse(IEnumerable<string> args)
        {
            return Parse(args);
        }
    }
}
