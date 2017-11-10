using Colipars.Attribute;
using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars
{
    public static class SetupExtensions
    {
        /// <summary>
        /// Creates a Parser using Attributes on the given types.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="configure"></param>
        /// <param name="optionTypes"></param>
        /// <returns></returns>
        public static AttributeParser Attributes(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null, params Type[] optionTypes)
        {
            var serviceProvider = ServiceProvider.Default;

            var configuration = new AttributeConfiguration(serviceProvider);
            serviceProvider.Register<Configuration>(configuration);
            serviceProvider.Register<AttributeConfiguration>(configuration);

            var settingsProvider = new AttributeSettingsProvider(configuration, optionTypes);
            var settings = settingsProvider.GenerateSettings();
            serviceProvider.Register<Settings>(settings);
            serviceProvider.Register<AttributeSettings>(settings);

            configure?.Invoke(configuration);

            return new AttributeParser(
                serviceProvider.GetService<AttributeSettings>(),
                serviceProvider.GetService<AttributeConfiguration>(),
                serviceProvider.GetService<IParameterFormatter>(),
                serviceProvider.GetService<IValueConverter>(),
                serviceProvider.GetService<IHelpPresenter>()
            );
        }

        public static AttributeParser Attributes<TOption>(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null)
        {
            return Attributes(instance, configure, typeof(TOption));
        }

        public static AttributeParser Attributes<TOption1, TOption2>(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null)
        {
            return Attributes(instance, configure, typeof(TOption1), typeof(TOption2));
        }

        public static AttributeParser Attributes<TOption1, TOption2, TOption3>(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null)
        {
            return Attributes(instance, configure, typeof(TOption1), typeof(TOption2), typeof(TOption3));
        }

        public static AttributeParser Attributes<TOption1, TOption2, TOption3, TOption4>(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null)
        {
            return Attributes(instance, configure, typeof(TOption1), typeof(TOption2), typeof(TOption3), typeof(TOption4));
        }
    }
}
