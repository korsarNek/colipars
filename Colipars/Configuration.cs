using Colipars.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Colipars
{
    public abstract class Configuration
    {
        /// <summary>
        /// Possible arguments to any verb that trigger the help.
        /// </summary>
        public IEnumerable<string> HelpArguments { get; set; } = new[] { "h", "help" };

        public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;

        public IVerb DefaultVerb { get; set; }

        //TODO: make it easier to replace the services we use.
        public IServiceProvider Services { get; }

        public abstract IEnumerable<IVerb> Verbs { get; }

        public Configuration(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public abstract IEnumerable<IOption> GetOptions(IVerb verb);
    }
}
