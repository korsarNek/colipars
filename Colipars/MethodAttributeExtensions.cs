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
        public static AttributeParser MethodAttributes(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null, params Type[] containerTypes)
        {
            var serviceProvider = ServiceProvider.Default;

            var configuration = new AttributeConfiguration(serviceProvider);
            serviceProvider.Register<Configuration>(configuration);
            serviceProvider.Register<AttributeConfiguration>(configuration);

            configuration.Initialize(containerTypes);

            configure?.Invoke(configuration);
            
            return new AttributeParser(
                serviceProvider.GetService<AttributeConfiguration>(),
                serviceProvider.GetService<IParameterFormatter>(),
                serviceProvider.GetService<IValueConverter>(),
                serviceProvider.GetService<IHelpPresenter>()
            );
        }

        public static AttributeParser MethodAttributes<TContainer>(this Parsers.SetupHelper instance, Action<AttributeConfiguration> configure = null)
        {
            return MethodAttributes(instance, configure, typeof(TContainer));
        }
    }
}
