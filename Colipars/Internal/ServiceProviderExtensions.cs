using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Internal
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider) where T : class
        {
            return (T)serviceProvider.GetService(typeof(T)) ?? throw new InvalidOperationException($"The service \"{typeof(T)}\" does not exist.");
        }
    }
}
