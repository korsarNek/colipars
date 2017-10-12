using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
using Colipars.Internal;

namespace Colipars.Attribute
{
    public class AttributeSettingsProvider
    {
        private IEnumerable<Type> _optionTypes;

        public Configuration Configuration { get; }

        public AttributeSettingsProvider(Configuration configuration, IEnumerable<Type> optionTypes)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _optionTypes = optionTypes ?? throw new ArgumentNullException(nameof(optionTypes));

            if (!optionTypes.Any())
                throw new ArgumentException($"\"{nameof(optionTypes)}\" is empty.");
        }

        public AttributeSettings GenerateSettings()
        {
            var verbSettings = new Dictionary<string, IEnumerable<InstanceOption>>();
            var verbConstructors = new Dictionary<string, ConstructorInfo>();
            foreach (var type in _optionTypes)
            {
                //check if type is valid
                if (type.IsGenericTypeDefinition)
                    throw new InvalidOperationException($"\"{type}\" is a generic type definition.");

                var constructor = type.GetConstructors().FirstOrDefault((x) => x.IsPublic && x.GetParameters().Length == 0);
                if (constructor == null)
                    throw new InvalidOperationException($"\"{type}\" doesn't have a public parameterless constructor.");

                string verbName = GetVerbFromType(type);

                verbConstructors.Add(verbName, constructor);

                //get options
                List<InstanceOption> instanceOptions = new List<InstanceOption>();
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var option = Validate(type,
                        property.GetCustomAttribute<NamedOptionAttribute>() ??
                        property.GetCustomAttribute<PositionalOptionAttribute>() ??
                        property.GetCustomAttribute<NamedCollectionOptionAttribute>() ??
                        (IOption)property.GetCustomAttribute<FlagOptionAttribute>()
                    );

                    if (option != null)
                        instanceOptions.Add(new InstanceOption(property, option));
                }

                verbSettings.Add(verbName, instanceOptions);
            }

            return new AttributeSettings(new ReadOnlyDictionary<string, IEnumerable<InstanceOption>>(verbSettings), new ReadOnlyDictionary<string, ConstructorInfo>(verbConstructors));

        }

        private IOption Validate(Type type, IOption option)
        {
            if (option == null)
                return null;

            if (Configuration.HelpArguments.Contains(option.Name))
                throw new AmbiguousMatchException($"The help argument \"{option.Name}\" is ambigous with option of the same name in \"{type}\"");

            return option;
        }

        public static string GetVerbFromType(Type type)
        {
            string verbName = type.Name;

            var verb = type.GetCustomAttribute<VerbAttribute>();
            if (verb?.Name != null)
            {
                verbName = verb.Name;
            }

            return verbName;
        }
    }
}
