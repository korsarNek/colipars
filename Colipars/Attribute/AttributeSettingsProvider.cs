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
            var verbSettings = new Dictionary<IVerb, IEnumerable<InstanceOption>>();
            var verbConstructors = new Dictionary<IVerb, ConstructorInfo>();
            foreach (var type in _optionTypes)
            {
                //check if type is valid
                if (type.IsGenericTypeDefinition)
                    throw new InvalidOperationException($"\"{type}\" is a generic type definition.");

                var constructor = type.GetConstructors().FirstOrDefault((x) => x.IsPublic && x.GetParameters().Length == 0);
                if (constructor == null)
                    throw new InvalidOperationException($"\"{type}\" doesn't have a public parameterless constructor.");

                var verb = GetVerbFromType(type);

                verbConstructors.Add(verb, constructor);

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

                verbSettings.Add(verb, instanceOptions);
            }

            return new AttributeSettings(new ReadOnlyDictionary<IVerb, IEnumerable<InstanceOption>>(verbSettings), new ReadOnlyDictionary<IVerb, ConstructorInfo>(verbConstructors));

        }

        private IOption Validate(Type type, IOption option)
        {
            if (option == null)
                return null;

            if (Configuration.HelpArguments.Contains(option.Name))
                throw new AmbiguousMatchException($"The help argument \"{option.Name}\" is ambigous with option of the same name in \"{type}\"");

            return option;
        }

        public static IVerb GetVerbFromType(Type type)
        {
            return type.GetCustomAttribute<VerbAttribute>() ?? new VerbAttribute(type.Name);
        }
    }
}
