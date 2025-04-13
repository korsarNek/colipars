using Colipars.Attribute;
using Colipars.Attribute.Class;
using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars
{
    public static class ClassAttributeExtensions
    {
#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        public static AttributeParser ClassAttributes(this Cli.SetupHelper instance, Action<AttributeConfiguration>? configure = null, params Type[] optionTypes)
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen
        {
            var serviceProvider = ServiceProvider.Default.Clone();

            var configuration = new AttributeConfiguration(serviceProvider, optionTypes);
            serviceProvider.Register<Configuration>(configuration);
            serviceProvider.Register<AttributeConfiguration>(configuration);

            //TODO: now that the verbs are on the configuration, wouldn't it make more sense to put it at a position after the parser has filled the config?
            configure?.Invoke(configuration);

            return new AttributeParser(
                serviceProvider.GetService<AttributeConfiguration>(),
                serviceProvider.GetService<IParameterFormatter>(),
                serviceProvider.GetService<IValueConverter>(),
                serviceProvider.GetService<IHelpPresenter>()
            );
        }

        public static IParser<SingleAttributeParseResult<TOption>> ClassAttributes<TOption>(this Cli.SetupHelper instance, Action<AttributeConfiguration>? configure = null) where TOption : class
        {
            return new SingleOptionAttributeParser<TOption>(ClassAttributes(instance, configure, typeof(TOption)));
        }

        public static IParser<AttributeParseResult> ClassAttributes<TOption1, TOption2>(this Cli.SetupHelper instance, Action<AttributeConfiguration>? configure = null)
        {
            return ClassAttributes(instance, configure, typeof(TOption1), typeof(TOption2));
        }

        public static IParser<AttributeParseResult> ClassAttributes<TOption1, TOption2, TOption3>(this Cli.SetupHelper instance, Action<AttributeConfiguration>? configure = null)
        {
            return ClassAttributes(instance, configure, typeof(TOption1), typeof(TOption2), typeof(TOption3));
        }

        public static IParser<AttributeParseResult> ClassAttributes<TOption1, TOption2, TOption3, TOption4>(this Cli.SetupHelper instance, Action<AttributeConfiguration>? configure = null)
        {
            return ClassAttributes(instance, configure, typeof(TOption1), typeof(TOption2), typeof(TOption3), typeof(TOption4));
        }
    }
}
