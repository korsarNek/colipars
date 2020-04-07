using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars.Attribute
{
    public class ServiceProvider : IServiceProvider, ICloneable
    {
        private static ServiceProvider? _defaultInstance = null;

        public static ServiceProvider Default
        {
            get
            {
                if (_defaultInstance == null)
                {
                    _defaultInstance = new ServiceProvider();

                    _defaultInstance.Register<IParameterFormatter>((_) => new Console.ParameterFormatter());
                    _defaultInstance.Register<IHelpPresenter>((services) => new Console.HelpPresenter(services.GetService<Configuration>(), services.GetService<IParameterFormatter>()));
                    _defaultInstance.Register<IValueConverter>((services) => new ValueTypeConverter(services.GetService<Configuration>().CultureInfo));
                    _defaultInstance.Register<IErrorPresenter>((_) => new Console.ErrorPresenter());
                    _defaultInstance.Register<ErrorHandler>((services) => new DefaultErrorHandler(services.GetService<IErrorPresenter>(), services.GetService<IHelpPresenter>()).HandleErrors);
                }

                return _defaultInstance;
            }
        }

        private readonly Dictionary<Type, object> _serviceInstances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, ServiceFactory<object>> _serviceFactories = new Dictionary<Type, ServiceFactory<object>>();

        public object? GetService(Type serviceType)
        {
            if (_serviceInstances.ContainsKey(serviceType))
                return _serviceInstances[serviceType];
            else if (_serviceFactories.ContainsKey(serviceType))
                return _serviceFactories[serviceType](this);

            return null;
        }

        public void Register<T>(T instance) where T : class
        {
            Register(instance, typeof(T));
        }

        public void Register(object instance, Type type)
        {
            if (!type.IsAssignableFrom(instance.GetType()))
            {
                throw new ArgumentException($"The type {type} is not assignable from {instance.GetType()}.");
            }
            _serviceInstances[type] = instance;
        }

        public void Register<T>(ServiceFactory<T> factory) where T : class
        {
            Register(factory, typeof(T));
        }

        public void Register(ServiceFactory<object> factory, Type type)
        {
            _serviceInstances.Remove(type);
            _serviceFactories[type] = factory;
        }

        public ServiceProvider Clone()
        {
            return (ServiceProvider)((ICloneable)this).Clone();
        }

        object ICloneable.Clone()
        {
            var provider = new ServiceProvider();
            foreach (var factory in _serviceFactories)
                provider.Register(factory.Value, factory.Key);

            foreach (var instance in _serviceInstances)
                provider.Register(instance.Value, instance.Key);

            return provider;
        }
    }

    public delegate T ServiceFactory<out T>(IServiceProvider services);
}
