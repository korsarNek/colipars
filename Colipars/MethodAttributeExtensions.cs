using Colipars.Attribute;
using Colipars.Attribute.Method;
using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars
{
    public static class MethodAttributeExtensions
    {
#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        public static AttributeParser MethodAttributes(this Parsers.SetupHelper setup, Action<AttributeConfiguration>? configure = null, object? instance = null, params Type[] containerTypes)
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen
        {
            var serviceProvider = ServiceProvider.Default;

            var configuration = new AttributeConfiguration(serviceProvider, containerTypes, instance);
            serviceProvider.Register<Configuration>(configuration);
            serviceProvider.Register<AttributeConfiguration>(configuration);

            configure?.Invoke(configuration);
            
            return new AttributeParser(
                serviceProvider.GetService<AttributeConfiguration>(),
                serviceProvider.GetService<IParameterFormatter>(),
                serviceProvider.GetService<IValueConverter>(),
                serviceProvider.GetService<IHelpPresenter>()
            );
        }

        public static AttributeParser MethodAttributes<TContainer>(this Parsers.SetupHelper setup, Action<AttributeConfiguration>? configure = null, TContainer? instance = null) where TContainer : class
        {
            return MethodAttributes(setup, configure, instance, typeof(TContainer));
        }
    }
}
