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
        private IHelpPresenter _helpPresenter;
        private AttributeHandler _attributeHandler;

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

            return AttributeParseResult.CreateHelpRequest(verb);
        }

        public AttributeParseResult ShowHelp()
        {
            _helpPresenter.Present();

            return AttributeParseResult.CreateHelpRequest();
        }

        #endregion

        protected AttributeParseResult CreateErrorResult(IVerb verb, IEnumerable<IError> errors)
        {
            return new AttributeParseResult(verb, Configuration.Services.GetService<IErrorHandler>(), errors);
        }

        public AttributeParseResult Parse(IEnumerable<string> args)
        {
            if (!_attributeHandler.TryParseSelectedVerb(Configuration.Verbs, ref args, out var error, out var verb, out var requestedHelp))
            {
                if (requestedHelp)
                    return ShowHelp(verb);

                return CreateErrorResult(verb, new[] { error });
            }

            if (!_attributeHandler.TryProcessArguments(verb, Configuration.GetOptions(verb), args, out var optionValues, out var errors))
            {
                return CreateErrorResult(verb, errors);
            }

            var verbData = Configuration.GetVerbData(verb);
            foreach (var optionValue in optionValues)
            {
                verbData.GetParameterValueOption(optionValue.Option).SetValue(optionValue.Value);
            }

            return new AttributeParseResult(verb, Configuration.Services.GetService<IErrorHandler>(), verbData.Method, verbData.Instance, verbData.ParameterOptions.Select((x) => x.Value).ToArray());
        }

        IParseResult IParser.Parse(IEnumerable<string> args)
        {
            return Parse(args);
        }
    }
}
