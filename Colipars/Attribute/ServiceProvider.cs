using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    public class ServiceProvider : IServiceProvider
    {
        private static ServiceProvider? _defaultInstance = null;

        public static ServiceProvider Default
        {
            get
            {
                if (_defaultInstance == null)
                {
                    _defaultInstance = new ServiceProvider();

                    _defaultInstance.Register<IParameterFormatter>(() => new Console.ParameterFormatter());
                    _defaultInstance.Register<IHelpPresenter>(() => new Console.HelpPresenter(_defaultInstance.GetService<Configuration>(), _defaultInstance.GetService<IParameterFormatter>()));
                    _defaultInstance.Register<IValueConverter>(() => new ValueTypeConverter(_defaultInstance.GetService<Configuration>().CultureInfo));
                    _defaultInstance.Register<IErrorPresenter>(() => new Console.ErrorPresenter());
                    _defaultInstance.Register<IErrorHandler>(() => new DefaultErrorHandler(_defaultInstance.GetService<IErrorPresenter>(), _defaultInstance.GetService<IHelpPresenter>()));
                }

                return _defaultInstance;
            }
        }

        private readonly Dictionary<Type, object> _serviceInstances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> _serviceFactories = new Dictionary<Type, Func<object>>();

        public object? GetService(Type serviceType)
        {
            if (_serviceInstances.ContainsKey(serviceType))
                return _serviceInstances[serviceType];
            else if (_serviceFactories.ContainsKey(serviceType))
                return _serviceFactories[serviceType]();

            return null;
        }

        public void Register<T>(T instance) where T : class
        {
            _serviceInstances[typeof(T)] = instance;
        }

        public void Register<T>(Func<T> factory) where T : class
        {
            _serviceInstances.Remove(typeof(T));
            _serviceFactories[typeof(T)] = () => factory();
        }
    }
}
