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
        private readonly IHelpPresenter _helpPresenter;
        private readonly AttributeHandler _attributeHandler;

        public AttributeParser(AttributeConfiguration configuration, IParameterFormatter parameterFormatter, IValueConverter valueConverter, IHelpPresenter helpPresenter)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _attributeHandler = new AttributeHandler(parameterFormatter, (option, value) =>
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
                if (Configuration._defaultVerb != null)
                    verb = Configuration._defaultVerb;
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
                    if (Configuration._defaultVerb != null)
                        verb = Configuration._defaultVerb;
                    else
                        return AttributeParseResult.CreateErrorResult(null, Configuration.Services.GetService<ErrorHandler>(), [new UnknownVerbError(firstParam)]);
                else
                    args = args.Skip(1);
            }

            if (args.Any((x) => Configuration.HelpArguments.Contains(x)))
                return ShowHelp(verb);

            if (!_attributeHandler.TryProcessArguments(verb, Configuration.GetOptions(verb), args, out var optionValues, out var errors))
                return AttributeParseResult.CreateErrorResult(verb, Configuration.Services.GetService<ErrorHandler>(), errors);

            var verbData = Configuration.GetVerbData(verb);

            foreach (var optionProperty in verbData.OptionProperties)
            {
                var providedOption = optionValues.FirstOrDefault((o) => o.Option == optionProperty.Option);
                if (providedOption == null)
                    continue;

                optionProperty.SetValue(verbData.Instance, providedOption.Value);
            }

            return AttributeParseResult.CreateSuccessResult(verb, Configuration.Services.GetService<ErrorHandler>(), verbData.Instance);
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

    public class SingleOptionAttributeParser<TOption> : IParser<SingleAttributeParseResult<TOption>> where TOption : class
    {
        private AttributeParser _attributeParser;

        public SingleOptionAttributeParser(AttributeParser attributeParser)
        {
            _attributeParser = attributeParser;
        }

        public SingleAttributeParseResult<TOption> Parse(IEnumerable<string> args)
        {
            return SingleAttributeParseResult<TOption>.FromAttributeParseResult(_attributeParser.Parse(args));
        }

        public SingleAttributeParseResult<TOption> ShowHelp(IVerb verb)
        {
            return SingleAttributeParseResult<TOption>.FromAttributeParseResult(_attributeParser.ShowHelp(verb));
        }

        public SingleAttributeParseResult<TOption> ShowHelp()
        {
            return SingleAttributeParseResult<TOption>.FromAttributeParseResult(_attributeParser.ShowHelp());
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
