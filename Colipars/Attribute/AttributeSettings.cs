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
        IReadOnlyDictionary<IVerb, IEnumerable<InstanceOption>> _verbsAndOptions;
        IReadOnlyDictionary<IVerb, ConstructorInfo> _verbConstructors;

        public AttributeSettings(IReadOnlyDictionary<IVerb, IEnumerable<InstanceOption>> verbsAndOptions, IReadOnlyDictionary<IVerb, ConstructorInfo> verbConstructors)
            : base(new ReadOnlyDictionary<IVerb, IEnumerable<IOption>>(verbsAndOptions.ToDictionary((x) => x.Key, (x) => x.Value.Select((o) => o.Option))) ?? throw new ArgumentNullException(nameof(verbsAndOptions)))
        {
            _verbsAndOptions = verbsAndOptions;
            _verbConstructors = verbConstructors;
        }

        public IEnumerable<InstanceOption> GetInstanceOptions(IVerb verb)
        {
            return _verbsAndOptions[verb];
        }

        public ConstructorInfo GetConstructor(IVerb verb)
        {
            return _verbConstructors[verb];
        }
    }
}
