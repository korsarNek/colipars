using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using System.Reflection;
using Colipars.Internal;

namespace Colipars.Attribute
{
    public class AttributeSettings : Settings
    {
        IReadOnlyDictionary<string, IEnumerable<InstanceOption>> _verbsAndOptions;
        IReadOnlyDictionary<string, ConstructorInfo> _verbConstructors;

        public AttributeSettings(IReadOnlyDictionary<string, IEnumerable<InstanceOption>> verbsAndOptions, IReadOnlyDictionary<string, ConstructorInfo> verbConstructors)
            : base(new ReadOnlyDictionary<string, IEnumerable<IOption>>(verbsAndOptions.ToDictionary((x) => x.Key, (x) => x.Value.Select((o) => o.Option))) ?? throw new ArgumentNullException(nameof(verbsAndOptions)))
        {
            _verbsAndOptions = verbsAndOptions;
            _verbConstructors = verbConstructors;
        }

        public IEnumerable<InstanceOption> GetInstanceOptions(string verb)
        {
            return _verbsAndOptions[verb];
        }

        public ConstructorInfo GetConstructor(string verb)
        {
            return _verbConstructors[verb];
        }
    }
}
