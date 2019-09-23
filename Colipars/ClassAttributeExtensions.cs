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
        public static AttributeParser ClassAttributes(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null, params Type[] optionTypes)
        {
            var serviceProvider = ServiceProvider.Default;

            var configuration = new AttributeConfiguration(serviceProvider);
            serviceProvider.Register<Configuration>(configuration);
            serviceProvider.Register<AttributeConfiguration>(configuration);

            configuration.Initialize(optionTypes);

            //TODO: now that the verbs are on the configuration, wouldn't it make more sense to put it at a position after the parser has filled the config?
            configure?.Invoke(configuration);

            return new AttributeParser(
                serviceProvider.GetService<AttributeConfiguration>(),
                serviceProvider.GetService<IParameterFormatter>(),
                serviceProvider.GetService<IValueConverter>(),
                serviceProvider.GetService<IHelpPresenter>()
            );
        }

        public static AttributeParser ClassAttributes<TOption>(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null)
        {
            return ClassAttributes(instance, configure, typeof(TOption));
        }

        public static AttributeParser ClassAttributes<TOption1, TOption2>(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null)
        {
            return ClassAttributes(instance, configure, typeof(TOption1), typeof(TOption2));
        }

        public static AttributeParser ClassAttributes<TOption1, TOption2, TOption3>(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null)
        {
            return ClassAttributes(instance, configure, typeof(TOption1), typeof(TOption2), typeof(TOption3));
        }

        public static AttributeParser ClassAttributes<TOption1, TOption2, TOption3, TOption4>(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null)
        {
            return ClassAttributes(instance, configure, typeof(TOption1), typeof(TOption2), typeof(TOption3), typeof(TOption4));
        }
    }
}
