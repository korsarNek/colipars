using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars
{
    //TODO: put Settings and Configuration together
    public class Settings
    {
        private IReadOnlyDictionary<IVerb, IEnumerable<IOption>> _verbsAndOptions;

        public IEnumerable<IVerb> Verbs { get => _verbsAndOptions.Keys; }

        public Settings(IReadOnlyDictionary<IVerb, IEnumerable<IOption>> verbsAndOptions)
        {
            _verbsAndOptions = verbsAndOptions;
        }

        public IEnumerable<IOption> GetOptions(IVerb verb)
        {
            if (verb == null)
                throw new ArgumentNullException(nameof(verb));

            if (!_verbsAndOptions.ContainsKey(verb))
                throw new KeyNotFoundException($"The verb \"{verb}\" is unknown.");

            return _verbsAndOptions[verb];
        }
    }
}
