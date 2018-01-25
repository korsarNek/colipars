using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.ObjectModel;
using Colipars.Internal;
using System.Collections;
using System.ComponentModel;

namespace Colipars.Attribute.Class
{
    public class AttributeParser : IParser<AttributeParseResult>
    {
        private IHelpPresenter _helpPresenter;
        private AttributeHandler _attributeHandler;

        public AttributeParser(AttributeConfiguration configuration, IParameterFormatter parameterFormatter, IValueConverter valueConverter, IHelpPresenter helpPresenter)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _attributeHandler = new AttributeHandler(configuration, parameterFormatter, (option, value) =>
            {
                var optionProperty = Configuration.GetOptionProperty(option);
                return valueConverter.ConvertFromString(option, optionProperty.PropertyInfo, AttributeHandler.GetValueType(optionProperty.Option, optionProperty.PropertyInfo.PropertyType), value);
            },
            (option) =>
            {
                return Configuration.GetOptionProperty(option).PropertyInfo.PropertyType;
            });
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

            foreach (var optionProperty in verbData.OptionProperties)
            {
                var providedOption = optionValues.FirstOrDefault((o) => o.Option == optionProperty.Option);
                if (providedOption == null)
                    continue;

                optionProperty.SetValue(verbData.Instance, providedOption.Value);
            }

            return new AttributeParseResult(verb, Configuration.Services.GetService<IErrorHandler>(), verbData.Instance);
        }

        IParseResult IParser.Parse(IEnumerable<string> args)
        {
            return Parse(args);
        }

        protected AttributeParseResult CreateErrorResult(IVerb verb, IEnumerable<IError> errors)
        {
            return new AttributeParseResult(verb, Configuration.Services.GetService<IErrorHandler>(), errors);
        }
    }
}
