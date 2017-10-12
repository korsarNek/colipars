using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colipars
{
    //TODO: put Settings and Configuration together
    public class Settings
    {
        private IReadOnlyDictionary<string, IEnumerable<IOption>> _verbsAndOptions;

        public IEnumerable<string> Verbs { get => _verbsAndOptions.Keys; }

        public Settings(IReadOnlyDictionary<string, IEnumerable<IOption>> verbsAndOptions)
        {
            _verbsAndOptions = verbsAndOptions;
        }

        public IEnumerable<IOption> GetOptions(string verb)
        {
            if (!_verbsAndOptions.ContainsKey(verb))
                throw new KeyNotFoundException($"The verb \"{verb}\" is unknown.");

            return _verbsAndOptions[verb];
        }
    }
}
