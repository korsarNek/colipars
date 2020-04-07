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

        public IVerb? DefaultVerb { get; set; }

        //TODO: make it easier to replace the services we use.
        public abstract IServiceProvider Services { get; }

        /// <summary>
        /// Whether to show the help if no verb was provided.
        ///
        /// Default is false.
        /// </summary>
        public bool ShowHelpOnMissingVerb { get; set; } = false;

        public abstract IEnumerable<IVerb> Verbs { get; }

        public abstract IEnumerable<IOption> GetOptions(IVerb verb);
    }
}
